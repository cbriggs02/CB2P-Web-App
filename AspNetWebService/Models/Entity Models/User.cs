using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AspNetWebService.Models.Entities
{
    /// <summary>
    ///     Represents the user entity, extending the IdentityUser class from ASP.NET Core Identity.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class User : IdentityUser
    {
        /// <summary>
        ///     Gets or sets the first name of the user.
        /// </summary>
        [Required]
        [PersonalData]
        [StringLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        ///     Gets or sets the last name of the user.
        /// </summary>
        [Required]
        [PersonalData]
        [StringLength(50)]
        public string LastName { get; set; }

        /// <summary>
        ///     Gets or sets the country of the user.
        /// </summary>
        [Required]
        [StringLength(75)]
        public string Country { get; set; }

        /// <summary>
        ///     Gets or sets the account status of the user.
        /// </summary>
        public int AccountStatus { get; set; }

        /// <summary>
        ///     Gets or sets the datetime this user was created at.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Gets or sets the datetime this user was last updated at.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        ///     Gets or sets a list of previous passwords used by this user.
        /// </summary>
        public virtual ICollection<PasswordHistory> Passwords { get; set; }
    }
}
