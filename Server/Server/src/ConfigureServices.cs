using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.HostedService;
using Server.Interface;

namespace Server
{
    public static class Configurator
    {
        public static IHostBuilder ConfigureServices(this IHostBuilder builder)
        {
            return builder.ConfigureServices((hostingContext, services) =>
                {
                    services.AddSingleton<ICookieHolder, CookieHolder>();
                    services.AddHostedService<CountDebugHostedService>();
                });
        }
    }
}
