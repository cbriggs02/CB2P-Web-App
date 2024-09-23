using AspNetWebService.Models.DataTransferObjectModels;

namespace AspNetWebService.Models.ApiResponseModels.UsersApiResponses
{
    /// <summary>
    ///     Represents the response returned by the users API when retrieving a user, or creating a user.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class UserApiResponse
    {
        /// <summary>
        ///     The User DTO object retrieved or created from the service that is returned by the API.
        ///     This will contain user details if successful.
        /// </summary>
        public UserDTO User { get; set; }
    }
}
