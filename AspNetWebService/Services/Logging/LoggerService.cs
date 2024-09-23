using AspNetWebService.Interfaces.Logging;

namespace AspNetWebService.Services.Logging
{
    /// <summary>
    ///     Provides a centralized logging service for various types of logging events. This service acts as a wrapper 
    ///     around the individual logging services for authorization breaches, exceptions, and performance.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class LoggerService : ILoggerService
    {
        private readonly IAuthorizationLoggerService _authorizationLoggerService;
        private readonly IExceptionLoggerService _exceptionLoggerService;
        private readonly IPerformanceLoggerService _performanceLoggerService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoggerService"/> class.
        /// </summary>
        /// <param name="authorizationLoggerService">
        ///     The service responsible for logging authorization breaches.
        /// </param>
        /// <param name="exceptionLoggerService">
        ///     The service responsible for logging exceptions.
        /// </param>
        /// <param name="performanceLoggerService">
        ///     The service responsible for logging slow performance events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the provided services are null.
        /// </exception>
        public LoggerService(IAuthorizationLoggerService authorizationLoggerService, IExceptionLoggerService exceptionLoggerService, IPerformanceLoggerService performanceLoggerService)
        {
            _authorizationLoggerService = authorizationLoggerService ?? throw new ArgumentNullException(nameof(authorizationLoggerService));
            _exceptionLoggerService = exceptionLoggerService ?? throw new ArgumentNullException(nameof(exceptionLoggerService));
            _performanceLoggerService = performanceLoggerService ?? throw new ArgumentNullException(nameof(performanceLoggerService));
        }


        /// <summary>
        ///     Asynchronously logs an authorization breach by the given user.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the authorization breach.
        /// </returns>
        public async Task LogAuthorizationBreach()
        {
            await _authorizationLoggerService.LogAuthorizationBreach();
        }


        /// <summary>
        ///     Asynchronously logs an exception that occurred during application execution.
        /// </summary>
        /// <param name="exception">
        ///     The exception object that was thrown.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the exception.
        /// </returns>
        public async Task LogException(Exception exception)
        {
            await _exceptionLoggerService.LogException(exception);
        }


        /// <summary>
        ///     Asynchronously logs a slow performance event based on the response time of a request.
        /// </summary>
        /// <param name="responseTime">
        ///     The response time in milliseconds of the request that triggered the slow performance event.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the slow performance event.
        /// </returns>
        public async Task LogSlowPerformance(long responseTime)
        {
            await _performanceLoggerService.LogSlowPerformance(responseTime);
        }
    }
}
