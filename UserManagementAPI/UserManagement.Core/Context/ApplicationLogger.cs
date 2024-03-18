using Microsoft.Extensions.Configuration;
using Serilog;
using ILogger = Serilog.ILogger;
using Serilog.Sinks.MSSqlServer;

namespace UserManagement.Core.Context
{
    public interface ILoggers
    {
        void LogInformation(string message, object details);
        void LogWarning(string message, object details);
        void LogError(string message, object details);
    }
    public class ApplicationLogger : ILoggers
    {
        ILogger _logger;
        IConfiguration _config;
        public ApplicationLogger(IConfiguration config)
        {
            _config = config;
            //_logger = new LoggerConfiguration()
            //      .WriteTo.SqlServer(_config["ConnectionStrings:DefaultConnection"],
            //                         sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs" })
            //      .CreateLogger();
        }
        public void LogError(string message, object details)
        {
            _logger.Error("Message: {@message}, Error : {@details}", message, details);
        }
        public void LogInformation(string message, object details)
        {
            _logger.Information("Message: {@message}, Info: {@ticketDetails}", message, details);
        }
        public void LogWarning(string message, object details)
        {
            _logger.Warning("Message: {@message}, Warning: {@ticketDetails}", message, details);
        }
    }
}
