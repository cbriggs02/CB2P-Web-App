using System.ComponentModel.DataAnnotations;

namespace AspNetWebService.Models.Request_Models
{
    /// <summary>
    ///     Represents the model for recording user passwords in history.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class PasswordHistoryRequest
    {
        /// <summary>
        ///     Gets or sets the id of the user who's password is being record in history in request.
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        /// <summary>
        ///     Gets or sets the hashed password being record in history in request.
        /// </summary>
        [Required(ErrorMessage = "Password hash is required")]
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }
    }
}
