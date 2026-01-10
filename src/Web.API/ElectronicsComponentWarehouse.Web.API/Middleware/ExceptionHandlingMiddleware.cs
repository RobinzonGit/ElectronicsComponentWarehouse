//Создаем middleware для глобальной обработки исключений
using System.Net;
using System.Text.Json;
using ElectronicsComponentWarehouse.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Web.API.Middleware
{
    /// <summary>
    /// Middleware для глобальной обработки исключений
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                error = new
                {
                    message = "An error occurred while processing your request.",
                    details = _env.IsDevelopment() ? exception.Message : null,
                    type = exception.GetType().Name
                },
                timestamp = DateTime.UtcNow,
                requestId = context.TraceIdentifier
            };

            switch (exception)
            {
                case EntityNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new
                    {
                        success = false,
                        error = new
                        {
                            message = exception.Message,
                            details = _env.IsDevelopment() ? exception.StackTrace : null,
                            type = exception.GetType().Name
                        },
                        timestamp = DateTime.UtcNow,
                        requestId = context.TraceIdentifier
                    };
                    break;

                case BusinessRuleException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        success = false,
                        error = new
                        {
                            message = exception.Message,
                            details = _env.IsDevelopment() ? exception.StackTrace : null,
                            type = exception.GetType().Name
                        },
                        timestamp = DateTime.UtcNow,
                        requestId = context.TraceIdentifier
                    };
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new
                    {
                        success = false,
                        error = new
                        {
                            message = "You are not authorized to perform this action.",
                            details = _env.IsDevelopment() ? exception.Message : null,
                            type = exception.GetType().Name
                        },
                        timestamp = DateTime.UtcNow,
                        requestId = context.TraceIdentifier
                    };
                    break;

                case ArgumentException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        success = false,
                        error = new
                        {
                            message = exception.Message,
                            details = _env.IsDevelopment() ? exception.StackTrace : null,
                            type = exception.GetType().Name
                        },
                        timestamp = DateTime.UtcNow,
                        requestId = context.TraceIdentifier
                    };
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new
                    {
                        success = false,
                        error = new
                        {
                            message = exception.Message,
                            details = _env.IsDevelopment() ? exception.StackTrace : null,
                            type = exception.GetType().Name
                        },
                        timestamp = DateTime.UtcNow,
                        requestId = context.TraceIdentifier
                    };
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response = new
                    {
                        success = false,
                        error = new
                        {
                            message = "An internal server error occurred.",
                            details = _env.IsDevelopment() ? exception.ToString() : null,
                            type = "InternalServerError"
                        },
                        timestamp = DateTime.UtcNow,
                        requestId = context.TraceIdentifier
                    };
                    break;
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _env.IsDevelopment()
            };

            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}