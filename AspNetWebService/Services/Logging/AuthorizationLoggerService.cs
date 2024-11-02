using AspNetWebService.Interfaces.Authentication;
using AspNetWebService.Interfaces.Logging;
using AspNetWebService.Models.RequestModels.Logging;

namespace AspNetWebService.Services.Logging
{
    /// <summary>
    ///     Service for logging authorization breaches.This service captures user-related information and logs unauthorized
    ///     access attempts using the <see cref="IAuditLoggerService"/>.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class AuthorizationLoggerService : IAuthorizationLoggerService
    {
        private readonly IAuditLoggerService _auditLogService;
        private readonly IUserContextService _userContextService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizationLoggerService"/> class.
        /// </summary>
        /// <param name="auditLogService">
        ///     The audit log service used for logging authorization breaches.
        /// </param>
        /// <param name="userContextService">
        ///     The user context service used to retrieve the current user's request path and IP address.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the parameters are null.
        /// </exception>
        public AuthorizationLoggerService(IAuditLoggerService auditLogService, IUserContextService userContextService)
        {
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        }


        /// <summary>
        ///     Asynchronously logs an authorization breach by capturing the user ID, action attempted, and the IP address.
        ///     This information is then sent as an audit log request to the audit log service.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the authorization breach.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when any of the provided services are null.
        /// </exception>
        public async Task LogAuthorizationBreach()
        {
            var principal = _userContextService.GetClaimsPrincipal();
            var currentUserId = _userContextService.GetUserId(principal);
            var requestPath = _userContextService.GetRequestPath();
            var ipAddress = _userContextService.GetAddress()?.ToString();

            if (currentUserId == null || requestPath == null || ipAddress == null)
            {
                throw new InvalidOperationException($"{nameof(currentUserId)} or {nameof(requestPath)} or {nameof(ipAddress)} cannot be null.");
            }

            var auditLogRequest = new AuditLogAuthorizationRequest
            {
                UserId = currentUserId,
                ActionAttempted = requestPath,
                IpAddress = ipAddress
            };

            await _auditLogService.LogAuthorizationBreach(auditLogRequest);
        }
    }
}
