using AspNetWebService.Models.Data_Transfer_Object_Models;
using AspNetWebService.Models.DataTransferObjectModels;

namespace AspNetWebService.Models.Data_Transfer_Object_Models
{
    /// <summary>
    ///     Represents the result of a user-related operation, including a list of users and pagination metadata.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class UserResult
    {
        /// <summary>
        ///     Gets or sets the list of users.
        /// </summary>
        public List<UserDTO> Users { get; set; }

        /// <summary>
        ///     Gets or sets the pagination metadata.
        /// </summary>
        public PaginationMetadata PaginationMetadata { get; set; }
    }
}
