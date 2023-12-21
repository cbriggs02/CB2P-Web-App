using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AspNetWebService.Models
{
    /// <summary>
    /// Represents the user entity, extending the IdentityUser class from ASP.NET Core Identity.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        [Required]
        [PersonalData]
        [StringLength(50)]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        [Required]
        [PersonalData]
        [StringLength(50)]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the birth date of the user.
        /// </summary>
        [PersonalData]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Checks if the birth date has been set for the user.
        /// </summary>
        /// <returns>True if the birth date is set and valid, otherwise false.</returns>
        public bool IsBirthDateSet()
        {
            return BirthDate > DateTime.MinValue;
        }
    }
}
