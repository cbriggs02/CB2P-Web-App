using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AspNetWebService.Models
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
        ///     Gets or sets the birth date of the user.
        /// </summary>
        [PersonalData]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? BirthDate { get; set; }

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

        /// <summary>
        ///     Checks if the birth date has been set for the user.
        /// </summary>
        /// <returns>
        ///     True if the birth date is set, otherwise false.
        /// </returns>
        public bool IsBirthDateSet()
        {
            return BirthDate.HasValue;
        }
    }
}
