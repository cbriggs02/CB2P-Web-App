using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CB2P_Web_App.Models
{
    /// <summary>
    /// Represents the application user with extended properties.
    /// Inherits from IdentityUser for ASP.NET Core Identity functionality.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50)]
        [DisplayName("First Name")]
        [PersonalData]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50)]
        [DisplayName("Last Name")]
        [PersonalData]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [StringLength(20)]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password of the user.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(8, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 8 characters")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the birth date of the user.
        /// </summary>
        [PersonalData]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
    }
}
