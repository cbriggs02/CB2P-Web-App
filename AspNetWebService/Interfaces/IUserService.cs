using AspNetWebService.Models.Data_Transfer_Object_Models;
using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.Request_Models.UserRequests;
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
        ///     Definition of a task used for retrieving list of users in system.
        /// </summary>
        /// <param name="request">
        ///     A model containing information used in request, such as a page number and page size.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a UserListResult object.
        /// </returns>
        Task<UserListResult> GetUsers(UserListRequest request);


        /// <summary>
        ///     Definition of a task used for retrieving a user by id in the system.
        /// </summary>
        /// <param name="id">
        ///     Id of user to be retrieved.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns UserResult object.
        /// </returns>
        Task<UserResult> GetUser(string id);


        /// <summary>
        ///     Definition of a task used for creating a new user in the system using user object.
        /// </summary>
        /// <param name="userDTO">
        ///     Data transfer object containing information used in user creation.
        /// </param>
        /// <returns>
        ///      A task representing the asynchronous operation that returns UserResult object.
        /// </returns>
        Task<UserResult> CreateUser(UserDTO userDTO);


        /// <summary>
        ///     Definition of a task used for updating a user in the system by id using user object.
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
        ///     Definition of a task used for deleting a user in the system by id.
        /// </summary>
        /// <param name="id">
        ///     Id used to locate user to be deleted in system.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns UserResult object.
        /// </returns>
        Task<UserResult> DeleteUser(string id);


        /// <summary>
        ///     Definition of a task used for activating a user in the system by id.
        /// </summary>
        /// <param name="id">
        ///     Id to identify user to be activated inside the system.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns UserResult object.
        /// </returns>
        Task<UserResult> ActivateUser(string id);


        /// <summary>
        ///     Definition of a task used for deactivating a user in the system by id.
        /// </summary>
        /// <param name="id">
        ///     Id of user who is being deactivated inside the system.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns UserResult object.
        /// </returns>
        Task<UserResult> DeactivateUser(string id);
    }
}
