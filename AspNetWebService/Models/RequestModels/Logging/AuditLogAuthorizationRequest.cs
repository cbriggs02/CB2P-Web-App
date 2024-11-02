namespace AspNetWebService.Models.RequestModels.Logging
{
    /// <summary>
    ///     Request model for logging audit data related to authorization breaches.
    ///     This model captures details of unauthorized access attempts.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class AuditLogAuthorizationRequest
    {
        /// <summary>
        ///     Gets or sets the unique identifier of the user who attempted the action.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        ///     Gets or sets the name or description of the action that was attempted.
        /// </summary>
        public string ActionAttempted { get; set; }

        /// <summary>
        ///     Gets or sets the IP address from which the action was attempted.
        /// </summary>
        public string IpAddress { get; set; }
    }
}
