using AspNetWebService.Interfaces.Authentication;
using AspNetWebService.Interfaces.Logging;
using AspNetWebService.Models.RequestModels.Logging;

namespace AspNetWebService.Services.Logging
{
    /// <summary>
    ///     Service for logging slow performance metrics. This service captures user-related information 
    ///     and logs slow response times using the <see cref="IAuditLoggerService"/>.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class PerformanceLoggerService : IPerformanceLoggerService
    {
        private readonly IAuditLoggerService _auditLogService;
        private readonly IUserContextService _userContextService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PerformanceLoggerService"/> class.
        /// </summary>
        /// <param name="auditLogService">
        ///     The audit log service used for logging slow performance metrics.
        /// </param>
        /// <param name="userContextService">
        ///     The user context service used to retrieve the current user's information such as request path and IP address.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the provided services are null.
        /// </exception>
        public PerformanceLoggerService(IAuditLoggerService auditLogService, IUserContextService userContextService)
        {
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        }


        /// <summary>
        ///    Asynchronously logs slow performance when a request takes longer than expected. Captures user details, the action attempted, 
        ///     the response time, and the IP address, and sends this information as an audit log request.
        /// </summary>
        /// <param name="reponseTime">
        ///     The response time of the request that took longer than the defined threshold, in milliseconds.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the slow performance event.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the current user ID, request path, or IP address is null.
        /// </exception>
        public async Task LogSlowPerformance(long reponseTime)
        {
            var principal = _userContextService.GetClaimsPrincipal();
            var currentUserId = _userContextService.GetUserId(principal);
            var ipAddress = _userContextService.GetAddress()?.ToString();
            var requestPath = _userContextService.GetRequestPath();

            if (currentUserId == null || ipAddress == null || requestPath == null)
            {
                throw new InvalidOperationException($"{nameof(currentUserId)} or {nameof(ipAddress)} or {nameof(requestPath)} cannot be null.");
            }

            var auditLogRequest = new AuditLogPerformanceRequest
            {
                UserId = currentUserId,
                Action = requestPath,
                ResponseTime = reponseTime,
                IpAddress = ipAddress
            };

            await _auditLogService.LogSlowPerformance(auditLogRequest);
        }
    }
}
