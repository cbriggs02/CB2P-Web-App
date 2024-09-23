
namespace AspNetWebService.Models.ServiceResultModels.PermissionResults
{
    /// <summary>
    ///     Represents the result of a permission-related operation performed by the permission service.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class PermissionServiceResult
    {
        /// <summary>
        ///     Indicates whether the permission operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Contains errors encountered during the permission operation, if any.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }
}
