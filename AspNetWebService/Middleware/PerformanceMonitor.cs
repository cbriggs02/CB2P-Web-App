using AspNetWebService.Interfaces.Logging;
using AspNetWebService.Services.Logging;
using System.Diagnostics;

namespace AspNetWebService.Middleware
{
    /// <summary>
    ///     Middleware for monitoring the performance of HTTP requests, including request duration and CPU usage.
    ///     This middleware captures performance metrics for every HTTP request, logs them, and provides insights for performance tuning.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class PerformanceMonitor
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitor> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PerformanceMonitor"/> class.
        /// </summary>
        /// <param name="next">
        ///     The delegate representing the next middleware in the request pipeline.
        /// </param>
        /// <param name="logger">
        ///     The logger instance for logging performance data.
        /// </param>
        /// <param name="scopeFactory">
        ///     The factory for creating service scopes to resolve scoped services.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public PerformanceMonitor(RequestDelegate next, ILogger<PerformanceMonitor> logger, IServiceScopeFactory scopeFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }


        /// <summary>
        ///     Asynchronously invokes the performance monitoring middleware.
        ///     Starts a timer, passes the request down the pipeline, and logs the request duration and CPU usage after completion.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="HttpContext"/> representing the current HTTP request.
        /// </param>
        /// <returns>
        /// <returns>
        ///     A task representing the asynchronous operation of processing the request.
        /// </returns>
        public async Task Invoke(HttpContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

            var requestId = Guid.NewGuid().ToString();
            var stopwatch = StartRequestTimer();

            await _next(context);

            var requestDuration = StopRequestTimer(stopwatch);
            var cpuUsage = GetCpuUsage();

            await CheckPerformance(requestDuration, loggerService);
            ConsoleLogPerformanceMetrics(context, requestId, requestDuration, cpuUsage);
        }


        /// <summary>
        ///     Starts the <see cref="Stopwatch"/> to measure the duration of the HTTP request.
        /// </summary>
        /// <returns>
        ///     An instance of a started <see cref="Stopwatch"/>.
        /// </returns>
        private static Stopwatch StartRequestTimer()
        {
            return Stopwatch.StartNew();
        }


        /// <summary>
        ///     Stops the <see cref="Stopwatch"/> and returns the elapsed time in milliseconds.
        /// </summary>
        /// <param name="stopwatch">
        ///     The <see cref="Stopwatch"/> instance used to measure the request duration.
        /// </param>
        /// <returns>
        ///     The total elapsed time of the request in milliseconds.
        /// </returns>
        private static long StopRequestTimer(Stopwatch stopwatch)
        {
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }


        /// <summary>
        ///     Retrieves the total CPU usage of the current process.
        /// </summary>
        /// <returns>
        ///     The total processor time consumed by the process in milliseconds.
        /// </returns>
        private static double GetCpuUsage()
        {
            return Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
        }


        /// <summary>
        ///     Asynchronously checks the performance of the request based on the request duration.
        ///     If the request duration exceeds the threshold (e.g., 1000ms), logs it via the logger service.
        /// </summary>
        /// <param name="requestDuration">
        ///     The total request processing time in milliseconds.
        /// </param>
        /// <param name="loggerService">
        ///     The service used for logging slow performance metrics.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        private static async Task CheckPerformance(long requestDuration, ILoggerService loggerService)
        {
            const int performanceThreshold = 1000;

            if (requestDuration > performanceThreshold)
            {
                await loggerService.LogSlowPerformance(requestDuration);
            }

        }


        /// <summary>
        ///     Logs the performance metrics, including request ID, request path, response status code, request duration, and CPU usage.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="HttpContext"/> containing details of the HTTP request and response.
        /// </param>
        /// <param name="requestId">
        ///     A unique identifier for the request.
        /// </param>
        /// <param name="requestDuration">
        ///     The total time taken to process the request in milliseconds.
        /// </param>
        /// <param name="cpuUsage">
        ///     The CPU time consumed by the request, in milliseconds.
        /// </param>
        private void ConsoleLogPerformanceMetrics(HttpContext context, string requestId, long requestDuration, double cpuUsage)
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
