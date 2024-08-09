using System.ComponentModel.DataAnnotations;

namespace AspNetWebService.Models.Request_Models.UserRequests
{
    /// <summary>
    ///     Represents the model for requesting list of users.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class UserListRequest
    {
        /// <summary>
        ///     Gets or sets the page of users being requested.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
        public int Page { get; set; }

        /// <summary>
        ///     Gets or sets the size of page of users being requested.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than 0.")]
        public int PageSize { get; set; }
    }
}
