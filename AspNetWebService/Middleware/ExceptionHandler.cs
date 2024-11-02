using AspNetWebService.Constants;
using AspNetWebService.Interfaces.Logging;
using Newtonsoft.Json;

namespace AspNetWebService.Middleware
{
    /// <summary>
    ///     Middleware for handling exceptions globally and providing a standardized error response.
    ///     Captures and logs detailed exception information while ensuring a consistent JSON error response for clients.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandler> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExceptionHandler"/> class.
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
        public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger, IServiceScopeFactory scopeFactory)
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
                await loggerService.LogException(ex);
                LogToConsole(context, ex);
                await WriteErrorResponse(context);
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
        private void LogToConsole(HttpContext context, Exception ex)
        {
            var (exceptionType, innerExceptionMessage, stackTrace, requestPath,
                requestQuery, requestMethod, timestamp) = GatherExceptionDetails(context, ex);

            _logger.LogError(ex, "{Message}. Exception of type {ExceptionType} occurred at {Timestamp}. " +
                "Request: {Method} {Path}{QueryString}, " +
                "Inner exception: {InnerExceptionMessage}, Stack Trace: {StackTrace}",
                "An unhandled exception occurred", exceptionType, timestamp, requestMethod, requestPath, requestQuery,
                innerExceptionMessage, stackTrace);
        }


        /// <summary>
        ///     Gathers detailed information about an exception and the associated HTTP request context.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="HttpContext"/> for the current request, providing information about the HTTP request.
        /// </param>
        /// <param name="ex">
        ///     The <see cref="Exception"/> that was thrown, containing details of the error.
        /// </param>
        /// <returns>
        ///   A list of exceptions details used when logging the exception to console.
        /// </returns>
        private static (string ExceptionType, string InnerExceptionMessage, string StackTrace, string RequestPath,
            string RequestQuery, string RequestMethod, DateTime Timestamp)
         GatherExceptionDetails(HttpContext context, Exception ex)
        {
            return (
                ExceptionType: ex.GetType().Name,
                InnerExceptionMessage: ex.InnerException?.Message ?? "No inner exception",
                StackTrace: ex.StackTrace ?? "No stack trace available",
                RequestPath: context.Request.Path.ToString() ?? "No request path",
                RequestQuery: context.Request.QueryString.ToString() ?? "No query string",
                RequestMethod: context.Request.Method ?? "No request method",
                Timestamp: DateTime.UtcNow
            );
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
        private static async Task WriteErrorResponse(HttpContext context)
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
