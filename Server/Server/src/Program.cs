using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using MagicOnion.Hosting;
using MagicOnion.Server;
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

            await Task.WhenAll(magicOnionHost.RunAsync());
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
    
}
