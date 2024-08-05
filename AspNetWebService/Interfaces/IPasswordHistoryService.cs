using AspNetWebService.Models;
using AspNetWebService.Models.Request_Models;

namespace AspNetWebService.Interfaces
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for PasswordHistory-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public interface IPasswordHistoryService
    {
        /// <summary>
        ///     Definition of service method that records history of passwords for a provided user.
        /// </summary>
        /// <param name="request">
        ///     A model object that contains required data for recording password in history, including user id and hashed password.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of saving the password history
        ///     record to the database. The task completes when the record has been successfully
        ///     saved, or an error occurs.
        /// </returns>
        Task AddPasswordHistory(PasswordHistoryRequest request);


        /// <summary>
        ///     Definition of service method that checks a users history for passwords that may be re-used.
        /// </summary>
        /// <param name="request">
        ///     A model object that contains required data for checking a users password history, including user id and hashed password.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is a boolean value indicating whether the provided password hash is found in the user's password history.
        ///     - <c>true</c> if the password hash is found in the user's history, indicating a potential re-use.
        ///     - <c>false</c> if the password hash is not found in the user's history, indicating that the password is not a re-used one.
        /// </returns>
        Task<bool> FindPasswordHash(PasswordHistoryRequest request);


        /// <summary>
        ///     Definition of a service method that deletes passwords kept in history as new ones are added to ensure every user only has 
        ///     their most recent 5 passwords stored in history.
        /// </summary>
        /// <param name="UserId">
        ///     Id of user whose passwords older then most recent 5 are being deleted from history.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of deleting password history
        ///     records in the database. The task completes when the record has been successfully
        ///     deleted, verified or an error occurs.
        /// </returns>
        Task RemoveOldPasswordHistories(string UserId);
    }
}
