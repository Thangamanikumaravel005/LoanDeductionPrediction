using System.Text.Json;

namespace LoanDeductionPrediction.API.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception occurred.");

                await HandleExceptionAsync(
                    context,
                    ex);
            }
        }

        private static async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            var statusCode =
                exception switch
                {
                    ArgumentException =>
                        StatusCodes.Status400BadRequest,

                    UnauthorizedAccessException =>
                        StatusCodes.Status401Unauthorized,

                    InvalidOperationException =>
                        StatusCodes.Status409Conflict,

                    KeyNotFoundException =>
                        StatusCodes.Status404NotFound,

                    _ =>
                        StatusCodes.Status500InternalServerError
                };

            var errorCode =
                exception switch
                {
                    ArgumentException =>
                        "VALIDATION_ERROR",

                    UnauthorizedAccessException =>
                        "UNAUTHORIZED",

                    InvalidOperationException =>
                        "BUSINESS_RULE_ERROR",

                    KeyNotFoundException =>
                        "NOT_FOUND",

                    _ =>
                        "INTERNAL_SERVER_ERROR"
                };

            var message = exception.Message;

            var response =
                new
                {
                    success = false,

                    statusCode,

                    errorCode,

                    message,

                    timestamp =
                        DateTime.UtcNow,

                    path =
                        context.Request.Path.ToString()
                };

            context.Response.StatusCode =
                statusCode;

            context.Response.ContentType =
                "application/json";

            var json =
                JsonSerializer.Serialize(
                    response,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy =
                            JsonNamingPolicy.CamelCase
                    });

            await context.Response.WriteAsync(
                json);
        }
    }
}