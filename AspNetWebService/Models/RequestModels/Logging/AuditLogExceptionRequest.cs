namespace IdentityServiceApi.Models.RequestModels.Logging
{
    /// <summary>
    ///     Model for capturing exception-related information for auditing purposes.
    ///     This request is used to log exceptions that occur during system operations,
    ///     along with the associated user and IP address details.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class AuditLogExceptionRequest
    {
        /// <summary>
        ///     Gets or sets the exception that occurred during the operation.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        ///     Gets or sets the user ID associated with the operation where the exception occurred.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        ///     Gets or sets the IP address from which the request originated.
        /// </summary>
        public string IpAddress { get; set; }
    }
}
