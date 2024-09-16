using System.ComponentModel.DataAnnotations;

namespace AspNetWebService.Models.Request_Models
{
    /// <summary>
    ///     Represents the model for searching user passwords in history.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class SearchPasswordHistoryRequest
    {
        /// <summary>
        ///     Gets or sets the id of the user who's password is being searched in history in request.
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        /// <summary>
        ///     Gets or sets the password being searched in history in request.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
