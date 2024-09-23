
namespace AspNetWebService.Models.ServiceResultModels.AuditLogServiceResults
{
    /// <summary>
    ///     Represents the result of a audit-log-related operation 
    ///     performed by the audit log service.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class AuditLogServiceResult
    {
        /// <summary>
        ///     Indicates whether the audit log operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Contains errors encountered during the audit log operation, if any.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }
}
