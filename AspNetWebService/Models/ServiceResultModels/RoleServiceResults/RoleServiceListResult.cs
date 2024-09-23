using AspNetWebService.Models.DataTransferObjectModels;

namespace AspNetWebService.Models.ServiceResultModels.RoleServiceResults
{
    /// <summary>
    ///     Represents the result of a service operation that retrieves a list of roles.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class RoleServiceListResult
    {
        /// <summary>
        ///     Gets or sets the collection of roles returned from the service operation.
        /// </summary>
        public IEnumerable<RoleDTO> Roles { get; set; } = new List<RoleDTO> ();
    }
}
