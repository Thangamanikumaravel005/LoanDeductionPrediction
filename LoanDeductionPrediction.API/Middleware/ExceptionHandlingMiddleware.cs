using System.Net;
using System.Text.Json;
using LoanDeductionPrediction.API.Models;

namespace LoanDeductionPrediction.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (ArgumentException ex)
            {
                await HandleExceptionAsync(
                    context,
                    ex,
                    HttpStatusCode.BadRequest,
                    "VALIDATION_ERROR");
            }
            catch (InvalidOperationException ex)
            {
                await HandleExceptionAsync(
                    context,
                    ex,
                    HttpStatusCode.Conflict,
                    "BUSINESS_RULE_ERROR");
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(
                    context,
                    ex,
                    HttpStatusCode.InternalServerError,
                    "INTERNAL_SERVER_ERROR");
            }
        }

        private async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception,
            HttpStatusCode statusCode,
            string errorCode)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. TraceId: {TraceId}",
                context.TraceIdentifier);

            context.Response.ContentType =
                "application/json";

            context.Response.StatusCode =
                (int)statusCode;

            var response = new ApiErrorResponse
            {
                Success = false,

                Message = exception.Message,

                ErrorCode = errorCode,

                TraceId = context.TraceIdentifier
            };

            var json = JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy =
                        JsonNamingPolicy.CamelCase
                });

            await context.Response.WriteAsync(json);
        }
    }
}