using AspNetWebService.Models.ServiceResultModels.RoleServiceResults;

namespace AspNetWebService.Interfaces.Authorization
{
    /// <summary>
    ///     Interface defining the contract for a service responsible for role-related operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public interface IRoleService
    {
        /// <summary>
        ///     Asynchronously retrieves a list of all roles available in the system.
        /// </summary>
        /// <returns>
        ///     A task that represents the asynchronous operation, containing a collection of role data transfer objects.
        /// </returns>
        Task<RoleServiceListResult> GetRoles();


        /// <summary>
        ///     Asynchronously creates a new role in the system with the specified name.
        /// </summary>
        /// <param name="roleName">
        ///     The name of the role to be created.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a result object indicating the outcome of the role creation.
        /// </returns>
        Task<RoleServiceResult> CreateRole(string roleName);


        /// <summary>
        ///     Asynchronously deletes a role identified by its ID from the system.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the role to be deleted.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a result object indicating the outcome of the role deletion.
        /// </returns>
        Task<RoleServiceResult> DeleteRole(string id);


        /// <summary>
        ///     Asynchronously assigns a specified role to a user identified by their ID.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to whom the role will be assigned.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role that is being assigned to the user.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a result object indicating the outcome of the role assignment.
        /// </returns>
        Task<RoleServiceResult> AssignRole(string id, string roleName);


        /// <summary>
        ///     Asynchronously removes a specified role to a user identified by their ID.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to whom the role will be removed.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role that is being removed to the user.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a result object indicating the outcome of the role removal.
        /// </returns>
        Task<RoleServiceResult> RemoveRole(string id, string roleName);
    }
}
