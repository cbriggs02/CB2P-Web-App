using AspNetWebService.Interfaces.Utilities;
using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.Entities;
using AspNetWebService.Models.ServiceResultModels.Authentication;
using AspNetWebService.Models.ServiceResultModels.Common;
using AspNetWebService.Models.ServiceResultModels.UserManagement;

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
        private readonly IParameterValidator _parameterValidator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceResultFactory"/> class.
        /// </summary>
        /// <param name="parameterValidator">
        ///     The parameter validator instance to enforce consistency.
        /// </param>
        public ServiceResultFactory(IParameterValidator parameterValidator)
        {
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
        }


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
            _parameterValidator.ValidateNotNullOrEmpty(token, nameof(token));
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
            _parameterValidator.ValidateObjectNotNull(user, nameof(user));
            ValidateUserProperties(user);

            return new UserServiceResult { Success = true, User = user };
        }


        /// <summary>
        ///     Creates a user lookup service result indicating a successful 
        ///     user lookup operation, including the details of the found user.
        /// </summary>
        /// <param name="user">
        ///     The user entity that was found during the lookup.
        /// </param>
        /// <returns>
        ///     A <see cref="UserLookupServiceResult"/> object indicating success 
        ///     and containing the found user.
        /// </returns>
        public UserLookupServiceResult UserLookupOperationSuccess(User user)
        {
            _parameterValidator.ValidateObjectNotNull(user, nameof(user));
            return new UserLookupServiceResult { Success = true, UserFound = user };
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
            ValidateErrors(errors);
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
            ValidateErrors(errors);
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
            ValidateErrors(errors);
            return new UserServiceResult { Success = false, Errors = errors.ToList() };
        }


        /// <summary>
        ///     Creates a user lookup service result indicating a failure 
        ///     in the user lookup operation, along with any associated error messages.
        /// </summary>
        /// <param name="errors">An array of error messages that describe the reasons 
        ///     for the failure of the user lookup operation.</param>
        /// <returns>
        ///     A <see cref="UserLookupServiceResult"/> object indicating failure 
        ///     and containing the list of error messages.
        /// </returns>
        public UserLookupServiceResult UserLookupOperationFailure(string[] errors)
        {
            ValidateErrors(errors);
            return new UserLookupServiceResult { Success = false, Errors = errors.ToList() };
        }


        /// <summary>
        ///     Validates the provided errors array for null values.
        /// </summary>
        /// <param name="errors">
        ///     An array of error messages to validate.
        /// </param>
        private void ValidateErrors(string[] errors)
        {
            _parameterValidator.ValidateObjectNotNull(errors, nameof(errors));
        }


        /// <summary>
        ///     Validates the properties of the given <see cref="UserDTO"/> to ensure all required fields are populated.
        /// </summary>
        /// <param name="user">
        ///     The <see cref="UserDTO"/> to validate.
        /// </param>
        private void ValidateUserProperties(UserDTO user)
        {
            _parameterValidator.ValidateNotNullOrEmpty(user.UserName, nameof(user.UserName));
            _parameterValidator.ValidateNotNullOrEmpty(user.FirstName, nameof(user.FirstName));
            _parameterValidator.ValidateNotNullOrEmpty(user.LastName, nameof(user.LastName));
            _parameterValidator.ValidateNotNullOrEmpty(user.Email, nameof(user.Email));
            _parameterValidator.ValidateNotNullOrEmpty(user.PhoneNumber, nameof(user.PhoneNumber));
            _parameterValidator.ValidateNotNullOrEmpty(user.Country, nameof(user.Country));
        }
    }
}
