
namespace AspNetWebService.Models.ServiceResultModels.PasswordResults
{
    /// <summary>
    ///     Represents the result of a password-related operation performed by the password service.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class PasswordServiceResult
    {
        /// <summary>
        ///     Indicates whether the password operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Contains errors encountered during the password operation, if any.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }
}
