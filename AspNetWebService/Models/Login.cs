using System.ComponentModel.DataAnnotations;

namespace AspNetWebService.Models
{
    /// <summary>
    /// Represents a model for user login credentials.
    /// </summary>
    public class Login
    {
        /// <summary>
        /// Gets or sets the username for login.
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password for login.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
