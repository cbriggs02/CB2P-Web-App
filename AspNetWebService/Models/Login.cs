using System.ComponentModel.DataAnnotations;

namespace AspNetWebService.Models
{
    /// <summary>
    ///     Represents the model for user login credentials.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class Login
    {
        /// <summary>
        ///     Gets or sets the username for authentication.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; }

        /// <summary>
        ///     Gets or sets the password for authentication.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
