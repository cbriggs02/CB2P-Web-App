using AspNetWebService.Models;
using AspNetWebService.Models.Data_Transfer_Object_Models;
using AspNetWebService.Models.DataTransferObjectModels;
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
        ///     
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
        Task<UserListResult> GetUsers(int page, int pageSize);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="id">
        ///     Id of user to be retrieved.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns UserResult object.
        /// </returns>
        Task<UserResult> GetUser(string id);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="userDTO">
        ///     Data transfur object containing information used in user creation.
        /// </param>
        /// <returns>
        ///      A task representing the asynchronous operation that returns UserResult object.
        /// </returns>
        Task<UserResult> CreateUser(UserDTO userDTO);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="id">
        ///     Id used to locate user in system to be updated.
        /// </param>
        /// <param name="userDTO">
        ///     Object model containing data to be adjusted in located user object model.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns UserResult object.
        /// </returns>
        Task<UserResult> UpdateUser(string id, UserDTO userDTO);

        /// <summary>
        ///     
        /// </summary>
        /// <param name="id">
        ///     id used to locate user to be deleted in system.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns UserResult object.
        /// </returns>
        Task<UserResult> DeleteUser(string id);
    }
}
