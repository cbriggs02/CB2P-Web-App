using AspNetWebService.Models.DataTransferObjectModels;

namespace AspNetWebService.Models.Result_Models
{
    /// <summary>
    ///     Repersents the result of a user-related operation, including a user DTO.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class UserResult
    {
        /// <summary>
        ///     Gets or sets the user DTO.
        /// </summary>
        public UserDTO User { get; set; }
    }
}
