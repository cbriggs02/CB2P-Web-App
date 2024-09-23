
namespace AspNetWebService.Models.ServiceResultModels.RoleServiceResults
{
    /// <summary>
    ///     Represents the result of a role-related operation performed by the role service.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class RoleServiceResult
    {
        /// <summary>
        ///     Indicates whether the role operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Contains errors encountered during the role operation, if any.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }
}
