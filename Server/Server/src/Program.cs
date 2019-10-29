using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Hosting;
using MagicOnion.HttpGateway.Swagger;
using MagicOnion.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Server
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());
            
            var config = BuildConfiguration(args);

            var magicOnionOptions = new MagicOnionOptions
            {
                MagicOnionLogger = new MagicOnionLogToGrpcLogger(),
                IsReturnExceptionStackTraceInErrorDetail = true
            };
            
            var magicOnionHost = MagicOnionHost.CreateDefaultBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, builder) => builder.AddConfiguration(config))
                .ConfigureLogging((hostingContext, logging) => BuildLogging(hostingContext.Configuration, logging))
                .UseMagicOnion(magicOnionOptions, BuildServerPortFromConfiguration(config.GetSection("MagicOnion")))
                .ConfigureServices()
                .UseConsoleLifetime()
                .Build();

            IWebHost? webHost = default;

            if (config.GetValue<bool>("Swagger:Enabled"))
            {
                var url = config["Swagger:UseUrl"];
                url = string.IsNullOrEmpty(url) ? "http://localhots:5432" : url;

                webHost = new WebHostBuilder()
                    .ConfigureServices(collection =>
                    {
                        collection.AddSingleton(magicOnionHost.Services
                            .GetService<MagicOnionHostedServiceDefinition>().ServiceDefinition);
                    })
                    .ConfigureLogging((hostingContext, logging) => BuildLogging(hostingContext.Configuration, logging))
                    .UseKestrel()
                    .ConfigureKestrel(serverOptions => serverOptions.AllowSynchronousIO = true)
                    .UseStartup<Startup>()
                    .UseUrls(url)
                    .Build();
            }

            await Task.WhenAll(magicOnionHost.RunAsync(), webHost?.RunAsync() ?? Task.CompletedTask);
        }

        private static IConfigurationRoot BuildConfiguration(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            var configRoot = Path.Combine(Directory.GetCurrentDirectory(), "config");
            configBuilder.SetBasePath(configRoot);

            var appsettingsList =
                Environment.GetEnvironmentVariable("CARDINAL_APPSETTINGS") ?? "appsettings.local.json";
            foreach (var appsettings in appsettingsList.Split(';').Where(s => !string.IsNullOrEmpty(s)))
            {
                configBuilder.AddJsonFile(appsettings);
            }

            configBuilder.AddEnvironmentVariables();
            configBuilder.AddCommandLine(args);
            return configBuilder.Build();
        }

        private static ServerPort BuildServerPortFromConfiguration(IConfiguration config)
        {
            var host = config["Host"] ?? "0.0.0.0";
            var port = int.TryParse(config["Port"], out var p) ? p : 12345;
            // TODO: credential

            return new ServerPort(host, port, ServerCredentials.Insecure);
        }

        private static void BuildLogging(IConfiguration configuration, ILoggingBuilder logging)
        {
            var loggingConfiguration = configuration.GetSection("Logging");
            logging.AddConfiguration(loggingConfiguration);

            switch (loggingConfiguration["Target"]?.ToLower())
            {
                case "console":
                case "":
                case null:
                    logging.AddConsole();
                    logging.AddDebug();
                    break;
                case "none":
                    break;
                default:
                    throw new Exception($"Unknown logging target: {loggingConfiguration["Target"]}");
            }
        }
    }
    
    // WebAPI Startup configuration.
    public class Startup
    {
        // Inject MagicOnionServiceDefinition from DIl
        public void Configure(IApplicationBuilder app, MagicOnionServiceDefinition magicOnion)
        {
            // Optional:Add Summary to Swagger
            // var xmlName = "Sandbox.NetCoreServer.xml";
            // var xmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), xmlName);

            // HttpGateway requires two middlewares.
            // One is SwaggerView(MagicOnionSwaggerMiddleware)
            // One is Http1-JSON to gRPC-MagicOnion gateway(MagicOnionHttpGateway)
            app.UseMagicOnionSwagger(magicOnion.MethodHandlers,
                new SwaggerOptions("MagicOnion.Server", "Swagger Integration Test", "/")
                {
                    // XmlDocumentPath = xmlPath
                });
            app.UseMagicOnionHttpGateway(magicOnion.MethodHandlers,
                new Channel("localhost:12345", ChannelCredentials.Insecure));
        }
    }
}
