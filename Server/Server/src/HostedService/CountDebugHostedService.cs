using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Interface;

namespace Server.HostedService
{
    public class CountDebugHostedService: BackgroundService
    {
        public static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(15) / 5;

        private ICookieHolder _cookieHolder;
        private ILogger<CountDebugHostedService> _logger;

        public CountDebugHostedService(ICookieHolder cookieHolder, ILogger<CountDebugHostedService> logger)
        {
            _cookieHolder = cookieHolder;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"NowCount:{_cookieHolder.NowCookieCount}");
                await Task.Delay(HeartbeatInterval, stoppingToken);
            }
        }

    }
}
