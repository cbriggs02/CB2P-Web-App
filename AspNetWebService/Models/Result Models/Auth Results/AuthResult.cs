namespace AspNetWebService.Models.Result_Models.Auth_Results
{
    /// <summary>
    ///     Represents the result of a authentication-related operation.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class AuthResult
    {
        /// <summary>
        ///     Used as token generated when user is authenticated.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        ///     Used to identify successful authentication operations.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Used to note errors during authentication.
        /// </summary>
        public List<string> Errors { get; set; }
    }
}
