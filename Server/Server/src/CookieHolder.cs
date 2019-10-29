using System;
using Microsoft.Extensions.Logging;
using Server.Interface;

namespace Server
{
    public sealed class CookieHolder : ICookieHolder
    {
        private int _nowCount = 0;
        private ILogger<CookieHolder> _logger;
        Object _gate = new object();
        public CookieHolder(ILogger<CookieHolder> logger)
        {
            _logger = logger;
            logger.LogInformation("CountHolder Constructor");
        }

        public int NowCookieCount
        {
            get
            {
                lock (_gate)
                {
                    return _nowCount;
                }
            }
        }


        public int AddCookie()
        {
            lock (_gate)
            {
                ++_nowCount;
                _logger.LogInformation($"Call SumCount {_nowCount}");
                return _nowCount;
            }
        }
    }
}
