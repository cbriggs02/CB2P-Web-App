using AspNetWebService.Models;
using AspNetWebService.Models.Result_Models.Auth_Results;

namespace AspNetWebService.Interfaces
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for authentication-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public interface IAuthService
    {
        /// <summary>
        ///     Definition of a task used for logging in a user in the system using credentials.
        /// </summary>
        /// <param name="credentials">
        ///     A model object that contains information required for authentication, this includes a username and password.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns AuthResult object.
        /// </returns>
        Task<AuthResult> Login(LoginRequest credentials);


        /// <summary>
        ///     Definition of a task used for logging out a user in the system.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation that returns AuthResult object.
        /// </returns>
        Task<AuthResult> Logout();
    }
}
