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
            catch (Exception ex)
            {
                LogExceptionDetails(context, ex);
                await WriteErrorResponse(context);
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
        private void LogExceptionDetails(HttpContext context, Exception ex)
        {
            var exceptionType = ex.GetType().Name;
            var innerExceptionMessage = ex.InnerException?.Message;
            var stackTrace = ex.StackTrace;
            var requestPath = context.Request.Path;
            var requestQuery = context.Request.QueryString;
            var requestMethod = context.Request.Method;
            var timestamp = DateTime.UtcNow;

            _logger.LogError(ex, "{Message}. Exception of type {ExceptionType} occurred at {Timestamp}. " +
                "Request: {Method} {Path}{QueryString}, " +
                "Inner exception: {InnerExceptionMessage}, Stack Trace: {StackTrace}",
                "An unhandled exception occurred", exceptionType, timestamp, requestMethod, requestPath, requestQuery,
                innerExceptionMessage, stackTrace);
        }


        /// <summary>
        ///     Writes an error response to the client, setting the status code to 500 (Internal Server Error)
        ///     and the content type to JSON. The response contains a standardized error message.
        /// </summary>
        /// <param name="context">
        ///     The HTTP context for the current request, used to manipulate the response.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of writing the response.
        /// </returns>
        private static async Task WriteErrorResponse(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            var response = new
            {
                error = "An unexpected error occurred. Please try again later."
            };
            var jsonResponse = JsonConvert.SerializeObject(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
