using AspNetWebService.Models.Entities;
using AspNetWebService.Models.ServiceResultModels.Common;

namespace AspNetWebService.Models.ServiceResultModels.UserManagement
{
    /// <summary>
    ///     Represents the result of a user lookup operation.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class UserLookupServiceResult : ServiceResult
    {
        /// <summary>
        ///     Gets or sets the user found during the lookup.
        /// </summary>
        public User UserFound { get; set; }
    }
}
