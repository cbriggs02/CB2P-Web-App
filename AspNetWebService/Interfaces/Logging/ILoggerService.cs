namespace AspNetWebService.Interfaces.Logging
{
    /// <summary>
    ///     Defines a contract for logging various application events, such as authorization breaches, exceptions, and performance metrics.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public interface ILoggerService
    {
        /// <summary>
        ///      Asynchronously logs an authorization breach by a specific user.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the authorization breach.
        /// </returns>
        Task LogAuthorizationBreach();


        /// <summary>
        ///      Asynchronously logs an exception that occurred within the application.
        /// </summary>
        /// <param name="exception">
        ///     The exception object that was thrown and should be logged.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the exception.
        /// </returns>
        Task LogException(Exception exception);


        /// <summary>
        ///      Asynchronously logs a slow performance event, typically associated with a long response time for a request.
        /// </summary>
        /// <param name="responseTime">
        ///     The response time, in milliseconds, that caused the slow performance event to be logged.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the slow performance event.
        /// </returns>
        Task LogSlowPerformance(long responseTime);
    }
}
