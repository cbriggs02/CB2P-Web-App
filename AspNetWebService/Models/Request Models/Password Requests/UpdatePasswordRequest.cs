using System.ComponentModel.DataAnnotations;

namespace AspNetWebService.Models.Request_Models
{
    /// <summary>
    ///     Represents the model for user updating password.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class UpdatePasswordRequest
    {
        /// <summary>
        ///     Gets or sets the current password of the user for request.
        /// </summary>
        [Required(ErrorMessage = "Current Password is required")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        /// <summary>
        ///     Gets or sets the new password of user for request.
        /// </summary>
        [Required(ErrorMessage = "New Password is required")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}
