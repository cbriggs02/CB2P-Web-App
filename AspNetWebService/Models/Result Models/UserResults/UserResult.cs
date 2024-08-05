using AspNetWebService.Models.DataTransferObjectModels;

namespace AspNetWebService.Models.Result_Models
{
    /// <summary>
    ///     Represents the result of a user-related operation, including a user DTO.
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

        /// <summary>
        ///     Used to identify successful user operations.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Used to note errors during user operations.
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        ///     Used as token generated when user logs in.
        /// </summary>
        public string Token { get; set; }
    }
}
