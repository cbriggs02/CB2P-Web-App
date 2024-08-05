using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetWebService.Models
{
    /// <summary>
    ///     Represents the password history entity, used to keep history of user passwords.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class PasswordHistory
    {
        /// <summary>
        ///     Gets or sets the id of the password being stored in history.
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the Id of the user who this password belongs too.
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        ///     Gets or sets the hashed password being stored in history.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        ///     Gets or sets the date this password was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        ///     Gets or sets the navigation property to the user who created this password.
        /// </summary>
        public virtual User User { get; set; }
    }
}
