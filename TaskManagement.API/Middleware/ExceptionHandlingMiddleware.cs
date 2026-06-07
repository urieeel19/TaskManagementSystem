using System.Net;
using System.Text.Json;

namespace TaskManagement.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during request execution.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError; // Default 500 error status code
            var message = "An unexpected error occurred on the server.";

            // Map custom application business layer exceptions to standardized HTTP status codes
            switch (exception)
            {
                case ArgumentException _:
                    code = HttpStatusCode.BadRequest; // 400 Bad Request
                    message = exception.Message;
                    break;
                case KeyNotFoundException _:
                    code = HttpStatusCode.NotFound; // 404 Not Found
                    message = exception.Message;
                    break;
                case UnauthorizedAccessException _:
                    code = HttpStatusCode.Unauthorized; // 401 Unauthorized
                    message = exception.Message;
                    break;
                case InvalidOperationException _:
                    code = HttpStatusCode.Conflict; // 409 Conflict (e.g., Username already taken)
                    message = exception.Message;
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            var result = JsonSerializer.Serialize(new { error = message });
            return context.Response.WriteAsync(result);
        }
    }
}
