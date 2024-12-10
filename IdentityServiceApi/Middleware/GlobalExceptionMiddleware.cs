using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Logging;
using Newtonsoft.Json;

namespace IdentityServiceApi.Middleware
{
    /// <summary>
    ///     Middleware for handling exceptions globally and providing a standardized error response.
    ///     Captures and logs detailed exception information while ensuring a consistent JSON error response for clients.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GlobalExceptionMiddleware"/> class.
        /// </summary>
        /// <param name="next">
        ///     The delegate representing the next middleware in the pipeline.
        /// </param>
        /// <param name="logger">
        ///     The logger instance for logging exceptions.
        /// </param>
        /// <param name="scopeFactory">
        ///     The factory for creating service scopes to resolve scoped services.
        /// </param>
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        ///     Asynchronously invokes the exception handling middleware. If an exception occurs during the HTTP request processing,
        ///     it is caught, logged, and a standardized JSON error response is returned to the client.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="HttpContext"/> representing the current HTTP request, providing access to request and response data.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of processing the request and handling any exceptions.
        /// </returns>
        public async Task Invoke(HttpContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await loggerService.LogException(ex); // Log exception in DB with audit logger
                ConsoleLogExceptionDetails(context, ex);
                await WriteServerErrorResponse(context); // Return 500 status to client
            }
        }

        /// <summary>
        ///     Logs detailed information about an exception, including the exception type, message, stack trace,
        ///     and HTTP request details, such as method and path. This helps diagnose issues effectively.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="HttpContext"/> for the current request, used to extract details like the request path and method.
        /// </param>
        /// <param name="ex">
        ///     The <see cref="Exception"/> that was thrown, including details like message, stack trace, and inner exception.
        /// </param>
        private void ConsoleLogExceptionDetails(HttpContext context, Exception ex)
        {
            var exceptionType = ex.GetType().Name;
            var innerExceptionMessage = ex.InnerException?.Message ?? "No inner exception";
            var stackTrace = ex.StackTrace ?? "No stack trace available";
            var requestPath = context.Request.Path.ToString() ?? "No request path";
            var requestQuery = context.Request.QueryString.ToString() ?? "No query string";
            var requestMethod = context.Request.Method ?? "No request method";
            var timestamp = DateTime.UtcNow;

            _logger.LogError(ex, "{Message}. Exception of type {ExceptionType} occurred at {Timestamp}. " +
                "Request: {Method} {Path}{QueryString}, " +
                "Inner exception: {InnerExceptionMessage}, Stack Trace: {StackTrace}",
                "An unhandled exception occurred", exceptionType, timestamp, requestMethod, requestPath, requestQuery,
                innerExceptionMessage, stackTrace);
        }

        /// <summary>
        ///     Asynchronously writes a standardized error response to the client with a 500 status code (Internal Server Error)
        ///     and a JSON body containing a generic error message.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="HttpContext"/> for the current request, used to manipulate the HTTP response.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of writing the error response to the client.
        /// </returns>
        private static async Task WriteServerErrorResponse(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = ErrorMessages.General.GlobalExceptionMessage
            };

            var jsonResponse = JsonConvert.SerializeObject(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
