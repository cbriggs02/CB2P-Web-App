namespace IdentityServiceApi.Models.RequestModels.Logging
{
    /// <summary>
    ///     Request model for logging audit data related to HTTP requests that exceed performance thresholds.
    ///     This model captures details of requests that experience slow response times, aiding in performance analysis.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class AuditLogPerformanceRequest
    {
        /// <summary>
        ///     Gets or sets the unique identifier of the user who made the request.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        ///     Gets or sets the name or description of the action that was performed.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        ///     Gets or sets the response time of the action as measured by the server, in milliseconds.
        /// </summary>
        public long ResponseTime { get; set; }

        /// <summary>
        ///     Gets or sets the IP address from which the request originated.
        /// </summary>
        public string IpAddress { get; set; }
    }
}
