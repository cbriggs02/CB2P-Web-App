using AspNetWebService.Interfaces.Utilities;
using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.ServiceResultModels;
using AspNetWebService.Models.ServiceResultModels.LoginServiceResults;
using AspNetWebService.Models.ServiceResultModels.UserServiceResults;

namespace AspNetWebService.Services.Utilities
{
    /// <summary>
    ///     Implements the <see cref="IServiceResultFactory"/> interface to create uniform service result 
    ///     objects for various operations within the application. This factory reduces code duplication 
    ///     by centralizing the result creation logic for service operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class ServiceResultFactory : IServiceResultFactory
    {
        /// <summary>
        ///     Creates a successful service result for general operations.
        /// </summary>
        /// <returns>
        ///     A <see cref="ServiceResult"/> indicating success.
        /// </returns>
        public ServiceResult GeneralOperationSuccess()
        {
            return new ServiceResult { Success = true  };
        }


        /// <summary>
        ///     Creates a successful login service result with a token.
        /// </summary>
        /// <param name="token">
        ///     The authentication token generated upon successful login.
        /// </param>
        /// <returns>
        ///     A <see cref="LoginServiceResult"/> containing the success status and the token.
        /// </returns>
        public LoginServiceResult LoginOperationSuccess(string token)
        {
            return new LoginServiceResult { Success = true, Token = token };
        }


        /// <summary>
        ///     Creates a successful user operation result with a user DTO.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="UserDTO"/> representing the user information.
        /// </param>
        /// <returns>
        ///     A <see cref="UserServiceResult"/> containing the success status and user data.
        /// </returns>
        public UserServiceResult UserOperationSuccess(UserDTO user)
        {
            return new UserServiceResult { Success = true, User = user };
        }


        /// <summary>
        ///     Creates a failed service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="ServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        public ServiceResult GeneralOperationFailure(string[] errors)
        {
            return new ServiceResult { Success = false, Errors = errors.ToList() };
        }


        /// <summary>
        ///     Creates a failed login service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="ServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        public LoginServiceResult LoginOperationFailure(string[] errors)
        {
            return new LoginServiceResult { Success = false, Errors = errors.ToList() };
        }


        /// <summary>
        ///     Creates a failed user service result with specified errors.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages describing the failure.
        /// </param>
        /// <returns>
        ///     A <see cref="ServiceResult"/> indicating failure along with the provided errors.
        /// </returns>
        public UserServiceResult UserOperationFailure(string[] errors)
        {
            return new UserServiceResult { Success = false, Errors = errors.ToList() };
        }
    }
}
