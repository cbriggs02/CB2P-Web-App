using AspNetWebService.Models.Data_Transfer_Object_Models;
using AspNetWebService.Models.Result_Models;

namespace AspNetWebService.Interfaces
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for user-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public interface IUserService
    {
        /// <summary>
        ///     Retrieves users with pagination metadata.
        /// </summary>
        /// <param name="page">
        ///     The page number.
        /// </param>
        /// <param name="pageSize">
        ///     The size of data to be returned per page.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a UserListResult object.
        /// </returns>
        Task<UserListResult> GetUsersAsync(int page, int pageSize);

        /// <summary>
        ///     Retrieves a user DTO in the database who matches the provided id.
        /// </summary>
        /// <param name="id">
        ///     Id of user to be retrieved.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns UserResult object.
        /// </returns>
        Task<UserResult> GetUserAsync(string id);
    }
}
