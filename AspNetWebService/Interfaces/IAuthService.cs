using AspNetWebService.Models.Result_Models;
using AspNetWebService.Models;

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
        /// <param name="login">
        ///     A model object that contains information required for authentication, this includes a username and password.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns UserResult object.
        /// </returns>
        Task<UserResult> Login(LoginRequest login);


        /// <summary>
        ///     Definition of a task used for logging out a user in the system.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation that returns UserResult object.
        /// </returns>
        Task<UserResult> Logout();
    }
}
