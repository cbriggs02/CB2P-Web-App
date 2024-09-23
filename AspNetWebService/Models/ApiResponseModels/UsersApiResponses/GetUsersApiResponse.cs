using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.PaginationModels;

namespace AspNetWebService.Models.ApiResponseModels.UsersApiResponses
{
    /// <summary>
    ///     Represents the response returned by the users API when retrieving users with pagination.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class GetUsersApiResponse
    {
        /// <summary>
        ///     The list of users retrieved from the service.
        /// </summary>
        public IEnumerable<UserDTO> Users { get; set; } = new List<UserDTO>();

        /// <summary>
        ///     Metadata for pagination, such as total count and page details.
        /// </summary>
        public PaginationMetadata PaginationMetadata { get; set; }
    }
}
