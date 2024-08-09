namespace AspNetWebService.Models.Result_Models.Password_Results
{
    /// <summary>
    ///     Represents the result of a password-related operation.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class PasswordResult
    {
        /// <summary>
        ///     Used to identify successful password operations.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Used to note errors during password operations.
        /// </summary>
        public List<string> Errors { get; set; }
    }
}
