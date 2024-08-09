using System.Diagnostics;

namespace AspNetWebService.Middleware
{
    /// <summary>
    ///     Middleware for monitoring the performance of HTTP requests.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class PerformanceMonitor
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitor> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PerformanceMonitor"/> class.
        /// </summary>
        /// <param name="next">
        ///     The delegate representing the next middleware in the pipeline.
        /// </param>
        /// <param name="logger">
        ///     The logger instance for logging performance metrics.
        /// </param>
        public PerformanceMonitor(RequestDelegate next, ILogger<PerformanceMonitor> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        ///     Invokes the performance monitoring middleware.
        /// </summary>
        /// <param name="context">
        ///     The HTTP context for the current request.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();

            await _next(context);

            stopwatch.Stop();
            var cpuUsage = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;

            _logger.LogInformation(
                $"Request ID: {requestId}, " +
                $"Request Path: {context.Request.Path}, " +
                $"Response Status Code: {context.Response.StatusCode}, " +
                $"Request Duration: {stopwatch.ElapsedMilliseconds} ms, " +
                $"CPU Usage: {cpuUsage} ms"
            );
        }
    }
}
