using AspNetWebService.Models.Request_Models;
using AspNetWebService.Models.Result_Models.Password_Results;

namespace AspNetWebService.Interfaces
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for password-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public interface IPasswordService
    {
        /// <summary>
        ///     Definition of a task used for setting a password for a user in the system by id.
        /// </summary>
        /// <param name="id">
        ///     Id to identify user to set password for inside the system.
        /// </param>
        /// <param name="request">
        ///     A model object that contains information required for setting password, this includes the password and the confirmed password.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns PasswordResult object.
        /// </returns>
        Task<PasswordResult> SetPassword(string id, SetPasswordRequest request);


        /// <summary>
        ///     Definition of a task used for updating the password of a user in the system by id.
        /// </summary>
        /// <param name="id">
        ///     Id to identify user to update password for inside the system.
        /// </param>
        /// <param name="request">
        ///     A model object that contains information required for updating password, this includes current and new password.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns PasswordResult object.
        /// </returns>
        Task<PasswordResult> UpdatePassword(string id, UpdatePasswordRequest request);
    }
}
