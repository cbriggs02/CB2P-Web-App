using IdentityServiceApi.Models.PaginationModels;
using IdentityServiceApi.Models.EntityModels;
using IdentityServiceApi.Models.DataTransferObjectModels;

namespace IdentityServiceApi.Models.ApiResponseModels.AuditLogsApiResponses
{
    /// <summary>
    ///     Represents the response returned by the audit log API when retrieving audit logs with pagination.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class GetAuditLogsApiResponse
    {
        /// <summary>
        ///     The list of audit logs retrieved from the service that is returned by the API.
        ///     This collection may be empty if no audit logs are available.
        /// </summary>
        public IEnumerable<AuditLogDTO> Logs { get; set; } = new List<AuditLogDTO>();

        /// <summary>
        ///     Metadata for pagination, such as total count and page details.
        /// </summary>
        public PaginationMetadata PaginationMetadata { get; set; }
    }
}
