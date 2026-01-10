//Создаем middleware для валидации модели
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ElectronicsComponentWarehouse.Web.API.Middleware
{
    /// <summary>
    /// Middleware для обработки ошибок валидации модели
    /// </summary>
    public class ModelValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ModelValidationMiddleware> _logger;

        public ModelValidationMiddleware(RequestDelegate next, ILogger<ModelValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (FluentValidation.ValidationException ex)
            {
                _logger.LogWarning("Validation failed: {Message}", ex.Message);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                var errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                var response = new
                {
                    success = false,
                    error = new
                    {
                        message = "Validation failed",
                        details = errors,
                        type = "ValidationError"
                    },
                    timestamp = DateTime.UtcNow,
                    requestId = context.TraceIdentifier
                };

                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                await context.Response.WriteAsync(jsonResponse);
            }
        }
    }
}