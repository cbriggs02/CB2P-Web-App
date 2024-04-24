using AspNetWebService.Models.Data_Transfer_Object_Models;

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
        ///     A task representing the asynchronous operation that returns a UserResult object.
        /// </returns>
        Task<UserResult> GetUsersAsync(int page, int pageSize);
    }
}
