
namespace AspNetWebService.Models.RequestModels.PasswordHistoryRequests
{
    /// <summary>
    ///     Represents the model for searching a user's password history,
    ///     used to determine if a specific password has been used previously.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class SearchPasswordHistoryRequest
    {
        /// <summary>
        ///     Gets or sets the id of the user who's password is being searched in history in 
        ///     request.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        ///     Gets or sets the password being searched in history in request.
        /// </summary>
        public string Password { get; set; }
    }
}
