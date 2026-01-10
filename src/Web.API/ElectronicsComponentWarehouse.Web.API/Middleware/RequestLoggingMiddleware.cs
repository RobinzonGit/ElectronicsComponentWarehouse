//Создаем middleware для логирования запросов
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Web.API.Middleware
{
    /// <summary>
    /// Middleware для логирования входящих HTTP-запросов
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            // Пропускаем запросы к swagger и health checks
            if (request.Path.StartsWithSegments("/swagger") ||
                request.Path.StartsWithSegments("/health"))
            {
                await _next(context);
                return;
            }

            var requestInfo = new
            {
                Method = request.Method,
                Path = request.Path,
                QueryString = request.QueryString.ToString(),
                ClientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                UserAgent = request.Headers.UserAgent.ToString()
            };

            _logger.LogInformation(
                "HTTP {Method} {Path}{QueryString} requested from {ClientIp} (User-Agent: {UserAgent})",
                requestInfo.Method,
                requestInfo.Path,
                requestInfo.QueryString,
                requestInfo.ClientIp,
                requestInfo.UserAgent);

            try
            {
                await _next(context);
                stopwatch.Stop();

                var response = context.Response;

                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                    request.Method,
                    request.Path,
                    response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception)
            {
                stopwatch.Stop();
                _logger.LogError(
                    "HTTP {Method} {Path} failed after {ElapsedMilliseconds}ms",
                    request.Method,
                    request.Path,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}