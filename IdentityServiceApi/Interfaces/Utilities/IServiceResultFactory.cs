using IdentityServiceApi.Models.DataTransferObjectModels;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.ServiceResultModels.Authentication;
using IdentityServiceApi.Models.ServiceResultModels.Common;
using IdentityServiceApi.Models.ServiceResultModels.UserManagement;

namespace IdentityServiceApi.Interfaces.Utilities
{
    /// <summary>
    ///     Provides methods for creating uniform service result objects 
    ///     for various operations within the application. This factory 
    ///     centralizes the creation logic for service results, ensuring 
    ///     consistency and reducing duplication in the codebase.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IServiceResultFactory
    {
        /// <summary>
        ///     Creates a service result indicating a successful operation 
        ///     without any additional data.
        /// </summary>
        /// <returns>
        ///     A <see cref="ServiceResult"/> object indicating success.
        /// </returns>
        ServiceResult GeneralOperationSuccess();


        /// <summary>
        ///     Creates a login service result indicating a successful login 
        ///     operation, along with the generated token for the session.
        /// </summary>
        /// <param name="token">
        ///     The authentication token generated for the user.
        /// </param>
        /// <returns>
        ///     A <see cref="LoginServiceResult"/> object indicating success.
        /// </returns>
        LoginServiceResult LoginOperationSuccess(string token);


        /// <summary>
        ///     Creates a user service result indicating a successful user 
        ///     operation, including the details of the user.
        /// </summary>
        /// <param name="user">
        ///     The user data transfer object containing user details.
        /// </param>
        /// <returns>
        ///     A <see cref="UserServiceResult"/> object indicating success.
        /// </returns>
        UserServiceResult UserOperationSuccess(UserDTO user);


        /// <summary>
        ///     Creates a user lookup service result indicating a successful 
        ///     user lookup operation, including the user entity found.
        /// </summary>
        /// <param name="user">
        ///     The user entity that was found during the lookup.
        /// </param>
        /// <returns>
        ///     A <see cref="UserLookupServiceResult"/> object indicating success.
        /// </returns>
        UserLookupServiceResult UserLookupOperationSuccess(User user);


        /// <summary>
        ///     Creates a service result indicating a general failure 
        ///     in an operation, including error messages.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="ServiceResult"/> object indicating failure.
        /// </returns>
        ServiceResult GeneralOperationFailure(string[] errors);


        /// <summary>
        ///     Creates a login service result indicating a failure during the 
        ///     login operation, including error messages.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="LoginServiceResult"/> object indicating failure.
        /// </returns>
        LoginServiceResult LoginOperationFailure(string[] errors);


        /// <summary>
        ///     Creates a user service result indicating a failure during the 
        ///     user operation, including error messages.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="UserServiceResult"/> object indicating failure.
        /// </returns>
        UserServiceResult UserOperationFailure(string[] errors);


        /// <summary>
        ///     Creates a user lookup service result indicating a failure during 
        ///     the user lookup operation, including error messages.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="UserLookupServiceResult"/> object indicating failure.
        /// </returns>
        UserLookupServiceResult UserLookupOperationFailure(string[] errors);
    }
}
