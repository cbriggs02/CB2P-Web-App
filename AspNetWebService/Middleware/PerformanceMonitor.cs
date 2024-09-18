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
            var requestId = Guid.NewGuid().ToString();
            var stopwatch = StartRequestTimer();

            await _next(context);

            var requestDuration = StopRequestTimer(stopwatch);
            var cpuUsage = GetCpuUsage();

            LogPerformanceMetrics(context, requestId, requestDuration, cpuUsage);
        }


        /// <summary>
        ///     Starts the stopwatch to measure request duration.
        /// </summary>
        /// <returns>
        ///     The started stopwatch.
        /// </returns>
        private static Stopwatch StartRequestTimer()
        {
            return Stopwatch.StartNew();
        }


        /// <summary>
        ///     Stops the stopwatch and returns the elapsed time in milliseconds.
        /// </summary>
        /// <param name="stopwatch">
        ///     The stopwatch used to measure the request duration.
        /// </param>
        /// <returns>
        ///     The elapsed time in milliseconds.
        /// </returns>
        private static long StopRequestTimer(Stopwatch stopwatch)
        {
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }


        /// <summary>
        ///     Retrieves the CPU usage of the current process.
        /// </summary>
        /// <returns>
        ///     The total CPU usage in milliseconds.
        /// </returns>
        private static double GetCpuUsage()
        {
            return Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
        }


        /// <summary>
        ///     Logs the performance metrics for the current request.
        /// </summary>
        /// <param name="context">
        ///     The HTTP context for the current request.
        /// </param>
        /// <param name="requestId">
        ///     The unique identifier for the request.
        /// </param>
        /// <param name="requestDuration">
        ///     The request duration in milliseconds.
        /// </param>
        /// <param name="cpuUsage">
        ///     The CPU usage in milliseconds.
        /// </param>
        private void LogPerformanceMetrics(HttpContext context, string requestId, long requestDuration, double cpuUsage)
        {
            _logger.LogInformation(
                $"Request ID: {requestId}, " +
                $"Request Path: {context.Request.Path}, " +
                $"Response Status Code: {context.Response.StatusCode}, " +
                $"Request Duration: {requestDuration} ms, " +
                $"CPU Usage: {cpuUsage} ms"
            );
        }
    }
}
