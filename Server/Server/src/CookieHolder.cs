using System;
using Microsoft.Extensions.Logging;
using Server.Interface;

namespace Server
{
    public sealed class CookieHolder : ICookieHolder
    {
        private int _nowCount = 0;
        private ILogger<CookieHolder> _logger;
        public CookieHolder(ILogger<CookieHolder> logger)
        {
            _logger = logger;
            logger.LogInformation("CountHolder Constructor");
        }

        public int NowCookieCount => _nowCount;

        public int AddCookie()
        {
            ++_nowCount;
            _logger.LogInformation($"Call SumCount {_nowCount}");
            return _nowCount;
        }
    }
}
