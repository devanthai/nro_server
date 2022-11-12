using System;
using NRO_Server.DatabaseManager;
using Serilog;
using Serilog.Events;

namespace NRO_Server.Logging
{
    public class ServerLogger : IServerLogger
    {
        private readonly ILogger _logger;

        public ServerLogger()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "{Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logging/log-.txt", LogEventLevel.Error, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public void Debug(string message)
        {
            if(Manager.gI().IsDebug) _logger.Information($"DEBUG ==> {message}");
        }
        
        public void Print(string message)
        {
            _logger.Information($"==> {message}");
        }

        public void Info(string info)
        {
            _logger.Information(info);
        }

        public void Warning(string message, Exception exception = null)
        {
            _logger.Warning(message, exception);
        }

        public void Error(string message, Exception exception)
        {
            _logger.Error(message, exception);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }
    }
}