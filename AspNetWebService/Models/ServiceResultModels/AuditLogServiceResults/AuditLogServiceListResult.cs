using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.PaginationModels;

namespace AspNetWebService.Models.ServiceResultModels.AuditLogServiceResults
{
    /// <summary>
    ///     Represents the result of a service operation that retrieves a list of audit logs.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class AuditLogServiceListResult
    {
        /// <summary>
        ///     Gets or sets the collection of audit logs returned from the service operation.
        /// </summary>
        public IEnumerable<AuditLogDTO> Logs { get; set; } = new List<AuditLogDTO>();

        /// <summary>
        ///     Gets or sets the pagination metadata associated with the audit log retrieval operation.
        ///     This metadata includes details such as total audit log count, current page, and page size. 
        /// </summary>
        public PaginationMetadata PaginationMetadata { get; set; }
    }
}
