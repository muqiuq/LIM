using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIM.Helpers
{
    internal class LoggerService
    {
        private static LoggerService _loggerService;

        public LoggerService() {

            Factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddEventLog();
            });
        }

        public ILoggerFactory Factory { get; }

        public static LoggerService Default { get
            {
                if (_loggerService == null)
                {
                    _loggerService = new LoggerService();
                }
                return _loggerService;
            } 
        }
        public static ILoggerFactory DefaultFactory
        {
            get
            {
                return Default.Factory;
            }
        }

    }
}
