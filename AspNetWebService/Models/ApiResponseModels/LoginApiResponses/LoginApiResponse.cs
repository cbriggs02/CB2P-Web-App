
namespace AspNetWebService.Models.ApiResponseModels.LoginApiResponses
{
    /// <summary>
    ///     Represents the response returned by the login API after a successful user login.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class LoginApiResponse
    {
        /// <summary>
        ///    The JWT (JSON Web Token) that is generated upon a successful login from login service.
        /// </summary>
        public string Token { get; set; }
    }
}
