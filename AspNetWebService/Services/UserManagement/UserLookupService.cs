using AspNetWebService.Constants;
using AspNetWebService.Interfaces.UserManagement;
using AspNetWebService.Interfaces.Utilities;
using AspNetWebService.Models.Entities;
using AspNetWebService.Models.ServiceResultModels.UserManagement;
using Microsoft.AspNetCore.Identity;

namespace AspNetWebService.Services.UserManagement
{
    /// <summary>
    ///     Provides services for looking up user information within the application. 
    ///     This service interacts with the underlying user management system to 
    ///     retrieve user data based on specified criteria and can be used across 
    ///     multiple modules in the application.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class UserLookupService : IUserLookupService
    {
        private readonly UserManager<User> _userManager;
        private readonly IParameterValidator _parameterValidator;
        private readonly IServiceResultFactory _serviceResultFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserLookupService"/> class.
        /// </summary>
        /// <param name="userManager">
        ///     The user manager used for managing user-related operations.
        /// </param>
        /// <param name="parameterValidator">
        ///     The paramter validator service used for defense checking service paramters.
        /// </param>
        /// <param name="serviceResultFactory">
        ///     The service used for creating the result objects being returned in operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the provided parameters are null.
        /// </exception>
        public UserLookupService(UserManager<User> userManager, IParameterValidator parameterValidator, IServiceResultFactory serviceResultFactory)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
            _serviceResultFactory = serviceResultFactory ?? throw new ArgumentNullException(nameof(serviceResultFactory));
        }


        /// <summary>
        ///     Asynchronously retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to be retrieved.
        /// </param>
        /// <returns>
        ///     A User lookup service result representing the asynchronous operation, 
        ///     containing the result of the user lookup which includes the user information 
        ///     if found, or error details if not.
        /// </returns>
        public async Task<UserLookupServiceResult> FindUserById(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var user = await _userManager.FindByIdAsync(id);
            return HandleLookupResult(user);
        }


        /// <summary>
        ///     Asynchronously retrieves a user by their user name.
        /// </summary>
        /// <param name="userName">
        ///     The username of the user to find.
        /// </param>
        /// <returns>
        ///     A User lookup service result representing the asynchronous operation, 
        ///     containing the result of the user lookup which includes the user information 
        ///     if found, or error details if not.
        /// </returns>
        public async Task<UserLookupServiceResult> FindUserByUsername(string userName)
        {
            _parameterValidator.ValidateNotNullOrEmpty(userName, nameof(userName));

            var user = await _userManager.FindByNameAsync(userName);
            return HandleLookupResult(user);
        }


        /// <summary>
        ///     Handles the result of a user lookup operation.
        /// </summary>
        /// <param name="user">
        ///     The user object retrieved from the user manager. If null, it indicates that the user was not found.
        /// </param>
        /// <returns>
        ///     A UserLookupServiceResult indicating the success or failure of the operation,
        ///     along with user information if found, or error details if not.
        /// </returns>
        private UserLookupServiceResult HandleLookupResult(User user)
        {
            if (user == null)
            {
                return _serviceResultFactory.UserLookupOperationFailure(new[] { ErrorMessages.User.NotFound });
            }

            return _serviceResultFactory.UserLookupOperationSuccess(user);
        }
    }
}
