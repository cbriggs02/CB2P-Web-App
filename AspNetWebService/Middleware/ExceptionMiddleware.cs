using Newtonsoft.Json;

namespace AspNetWebService.Middleware
{
    /// <summary>
    ///     Middleware for handling exceptions globally and providing a consistent error response.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExceptionMiddleware"/> class.
        /// </summary>
        /// <param name="next">
        ///     The delegate representing the next middleware in the pipeline.
        /// </param>
        /// <param name="logger">
        ///     The logger instance for logging exceptions.
        /// </param>
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }


        /// <summary>
        ///     Invokes the exception handling middleware. This method attempts to process the HTTP request,
        ///     and if an exception is thrown, it catches it, logs detailed information, and sends a standardized
        ///     error response to the client.
        /// </summary>
        /// <param name="context">
        ///     The HTTP context for the current request, providing access to request and response data.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentNullException ex)
            {
                LogExceptionDetails(context, ex, "An argument null exception occurred during controller instantiation.");
                await WriteErrorResponseAsync(context, "An unexpected error occurred during controller instantiation.");
            }
            catch (HttpRequestException ex)
            {
                LogExceptionDetails(context, ex, "An HTTP request exception occurred.");
                await WriteErrorResponseAsync(context, "An error occurred while processing the HTTP request.");
            }
            catch (UnauthorizedAccessException ex)
            {
                LogExceptionDetails(context, ex, "An unauthorized access exception occurred.");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await WriteErrorResponseAsync(context, "You do not have permission to access this resource.");
            }
            catch (TaskCanceledException ex)
            {
                LogExceptionDetails(context, ex, "A task was canceled.");
                await WriteErrorResponseAsync(context, "The request was canceled.");
            }
            catch (InvalidOperationException ex)
            {
                LogExceptionDetails(context, ex, "An invalid operation exception occurred.");
                await WriteErrorResponseAsync(context, "An invalid operation was attempted.");
            }
            catch (Exception ex)
            {
                LogExceptionDetails(context, ex, "An unhandled exception occurred.");
                await WriteErrorResponseAsync(context, "An unexpected error occurred.");
            }
        }


        /// <summary>
        ///     Logs detailed information about an exception, including the type of exception, user information, request details,
        ///     environment, and other relevant data. This method helps in diagnosing issues by providing a comprehensive log entry.
        /// </summary>
        /// <param name="context">
        ///     The HTTP context for the current request, used to extract details such as the user and request path.
        /// </param>
        /// <param name="ex">
        ///     The exception that was thrown, providing details such as the message, stack trace, and inner exception.
        /// </param>
        /// <param name="message">
        ///     A custom message providing context about where and why the exception occurred.
        /// </param>
        private void LogExceptionDetails(HttpContext context, Exception ex, string message)
        {
            var exceptionType = ex.GetType().Name;
            var innerExceptionMessage = ex.InnerException?.Message;
            var stackTrace = ex.StackTrace;
            var user = context.User?.Identity?.Name ?? "Anonymous";
            var requestPath = context.Request.Path;
            var requestQuery = context.Request.QueryString;
            var requestMethod = context.Request.Method;
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
            var timestamp = DateTime.UtcNow;

            _logger.LogError(ex, "{Message}. Exception of type {ExceptionType} occurred at {Timestamp}. " +
                "User: {User}, Request: {Method} {Path}{QueryString}, Environment: {Environment}, " +
                "Inner exception: {InnerExceptionMessage}, Stack Trace: {StackTrace}",
                message, exceptionType, timestamp, user, requestMethod, requestPath, requestQuery,
                environment, innerExceptionMessage, stackTrace);
        }


        /// <summary>
        ///     Writes an error response to the client, setting the status code to 500 (Internal Server Error)
        ///     and the content type to JSON. The response contains a standardized error message.
        /// </summary>
        /// <param name="context">
        ///     The HTTP context for the current request, used to manipulate the response.
        /// </param>
        /// <param name="errorMessage">
        ///     The message to be included in the error response, typically describing the nature of the error.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of writing the response.
        /// </returns>
        private static async Task WriteErrorResponseAsync(HttpContext context, string errorMessage)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            var response = JsonConvert.SerializeObject(new { error = errorMessage });
            await context.Response.WriteAsync(response);
        }
    }
}
