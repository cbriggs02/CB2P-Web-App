using AspNetWebService.Models.DataTransferObjectModels;

namespace AspNetWebService.Models.ServiceResultModels.UserServiceResults
{
    /// <summary>
    ///     Represents the result of a user-related operation 
    ///     performed by the user service, including a user DTO.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class UserServiceResult : ServiceResult
    {
        /// <summary>
        ///     The user DTO object obtained from the operation.
        ///     This may be empty if no users match the request criteria.
        /// </summary>
        public UserDTO User { get; set; }
    }
}
