using AspNetWebService.Models.RequestModels.PasswordHistoryRequests;

namespace AspNetWebService.Interfaces.UserManagement
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
        ///     Asynchronously records the history of passwords for a specified user.
        /// </summary>
        /// <param name="request">
        ///     A model object that contains the necessary data for recording a password in history,
        ///     including the user ID and the hashed password to be stored.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of saving the password history record
        ///     to the database. The task completes when the record has been successfully saved, or if an error occurs.
        /// </returns>
        Task AddPasswordHistory(StorePasswordHistoryRequest request);


        /// <summary>
        ///     Asynchronously checks the user's password history for potentially reused passwords.
        /// </summary>
        /// <param name="request">
        ///     A model object that contains the required data for checking a user's password history,
        ///     including the user ID and the password to be checked.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is a boolean value indicating whether
        ///     the provided password hash is found in the user's password history.
        ///     - true if the password hash is found in the user's history, indicating a potential reuse.
        ///     - false if the password hash is not found in the user's history, indicating that the password is not reused.
        /// </returns>
        Task<bool> FindPasswordHash(SearchPasswordHistoryRequest request);


        /// <summary>
        ///     Asynchronously removes the password history for a specified user.
        /// </summary>
        /// <param name="userId">
        ///     The unique identifier of the user whose password history is being removed.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is a boolean value indicating whether
        ///     the password history has been successfully deleted.
        ///     - true if the password history was successfully deleted for the specified user ID.
        ///     - false if the password history could not be deleted for the specified user ID.
        /// </returns>
        Task<bool> DeletePasswordHistory(string userId);
    }
}
