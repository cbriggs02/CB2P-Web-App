using Newtonsoft.Json;

namespace AspNetWebService.Middleware
{
    /// <summary>
    ///     Middleware for handling exceptions globally and providing a consistent error response.
    ///     @Author: Christian Briglio
    /// </summary>
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
        ///     Invokes the exception handling middleware.
        /// </summary>
        /// <param name="context">
        ///     The HTTP context for the current request.
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
                _logger.LogError(ex, "An argument null exception occurred during controller instantiation.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                // Serialize error response to JSON
                var response = JsonConvert.SerializeObject(new { error = "An unexpected error occurred during controller instantiation." });
                await context.Response.WriteAsync(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                // Serialize error response to JSON
                var response = JsonConvert.SerializeObject(new { error = "An unexpected error occurred." });
                await context.Response.WriteAsync(response);
            }
        }
    }
}
