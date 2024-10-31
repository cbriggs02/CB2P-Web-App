using AspNetWebService.Interfaces.Authentication;
using AspNetWebService.Interfaces.Logging;
using AspNetWebService.Interfaces.Utilities;
using AspNetWebService.Models.RequestModels.AuditLogRequests;

namespace AspNetWebService.Services.Logging
{
    /// <summary>
    ///     Service for logging exceptions and sending audit log requests.
    ///     This service is responsible for capturing exception details and user context information
    ///     and logging them using the <see cref="IAuditLoggerService"/>.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class ExceptionLoggerService : IExceptionLoggerService
    {
        private readonly IAuditLoggerService _auditLogService;
        private readonly IUserContextService _userContextService;
        private readonly IParameterValidator _parameterValidator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExceptionLoggerService"/> class.
        /// </summary>
        /// <param name="auditLogService">
        ///     The audit log service used for logging exception details.
        /// </param>
        /// <param name="userContextService">
        ///     The user context service used to retrieve the current user ID and IP address.
        /// </param>
        /// <param name="parameterValidator">
        ///     The paramter validator service used for defense checking service paramters.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the provided services are null.
        /// </exception>
        public ExceptionLoggerService(IAuditLoggerService auditLogService, IUserContextService userContextService, IParameterValidator parameterValidator)
        {
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
        }


        /// <summary>
        ///     Asynchronously logs an exception by capturing details about the exception, current user, and IP address.
        ///     The exception information is then sent as an audit log request to the audit log service.
        /// </summary>
        /// <param name="exception">
        ///     The exception that occurred and needs to be logged.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the exception.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the current user ID or IP address is null.
        /// </exception>
        public async Task LogException(Exception exception)
        {
            _parameterValidator.ValidateObjectNotNull(exception, nameof(exception));

            var principal = _userContextService.GetClaimsPrincipal();
            var currentUserId = _userContextService.GetUserId(principal);
            var ipAddress = _userContextService.GetAddress()?.ToString();

            if (currentUserId == null || ipAddress == null)
            {
                throw new InvalidOperationException($"{nameof(currentUserId)} or {nameof(ipAddress)} cannot be null.");
            }

            var auditLogRequest = new AuditLogExceptionRequest
            {
                Exception = exception,
                UserId = currentUserId,
                IpAddress = ipAddress
            };

            await _auditLogService.LogException(auditLogRequest);
        }
    }
}
