using Serilog;
using ILogger = Serilog.ILogger;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;

namespace UserManagement.Core.Context
{
    public interface ILoggers
    {
        public Task LogAsync(string message, LogLevel logLevel);
    }
    public class ApplicationLogger : ILoggers
    {
        ILogger _logger;
        IHttpContextAccessor _contextAccessor;
        public ApplicationLogger(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
            _logger = new LoggerConfiguration()
                 .WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] Client IP: {ClientIP} Client Name: {ClientName} Host: {Host} Method: {Method} Params: {Params} Message: {Message}{NewLine}{Exception}")
            .CreateLogger();
        }

        public async Task LogAsync(string message, LogLevel logLevel)
        {
            var serilogLevel = ConvertToSerilogLevel(logLevel);

            var request = _contextAccessor.HttpContext.Request;
            var clientIP = _contextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
            var clientName = request.Headers["User-Agent"];
            var hostName = Environment.MachineName;
            var requestParameters = GetRequestParameters(request);

            var httpContext = _contextAccessor.HttpContext;
            var controllerActionDescriptor = httpContext?.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
            var controllerName = controllerActionDescriptor?.ControllerName;
            var actionName = controllerActionDescriptor?.ActionName;
            var apiMethod = $"{controllerName}/{actionName}";

            Log.Write(serilogLevel, "LogLevel: {LogLevel}, Time: {Time}, Client IP: {ClientIP}, Client Name: {ClientName}, Host Name: {HostName}, API Method: {ApiMethodName}, Request Content: {RequestContent}, Message: {Message}",
                logLevel,
                DateTime.UtcNow,
                clientIP,
                clientName,
                hostName,
                apiMethod,
                requestParameters,
                message);
        }
        private Serilog.Events.LogEventLevel ConvertToSerilogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Information => Serilog.Events.LogEventLevel.Information,
                LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
                LogLevel.Error => Serilog.Events.LogEventLevel.Error,
                _ => Serilog.Events.LogEventLevel.Information,
            };
        }

        public async Task<string> GetRequestParameters(HttpRequest request)
        {
            string requestContent = ""; // This variable will hold either the parameters or the body content

            if (request.Method == "GET")
            {
                requestContent = string.Join("&", request.Query.Select(q => $"{q.Key}={q.Value}"));
            }
            else if (request.Method == "POST" && request.ContentType != null && request.ContentType.Contains("application/json"))
            {
                request.EnableBuffering();

                using (var reader = new StreamReader(request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
                {
                    requestContent = await reader.ReadToEndAsync();
                    request.Body.Position = 0;
                }
            }
            else
            {
                // For POST (or other methods) where body is not read or is not application/json
                requestContent = "Request body not logged or non-JSON content type";
            }
            return requestContent;
        }

    }
}
