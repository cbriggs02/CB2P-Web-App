using System.ComponentModel.DataAnnotations;

namespace AspNetWebService.Models.Request_Models
{
    /// <summary>
    ///     Represents the model for user setting password.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class SetPasswordRequest
    {
        /// <summary>
        ///     Gets or sets the password user is requesting for credentials.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets the confirmed password user is requesting for credentials.
        /// </summary>
        [Required(ErrorMessage = "Confirmed Password is required")]
        [DataType(DataType.Password)]
        public string PasswordConfirmed { get; set; }
    }
}
