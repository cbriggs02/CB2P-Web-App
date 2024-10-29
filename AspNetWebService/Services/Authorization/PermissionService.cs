using AspNetWebService.Constants;
using AspNetWebService.Interfaces.Authorization;
using AspNetWebService.Interfaces.Logging;
using AspNetWebService.Models.ServiceResultModels.PermissionResults;

namespace AspNetWebService.Services.Authorization
{
    /// <summary>
    ///     Service responsible for interacting with authorization-related data and business logic.
    ///     This service encapsulates the interaction between other services and the auth service.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class PermissionService : IPermissionService
    {
        private readonly IAuthorizationService _authService;
        private readonly ILoggerService _loggerService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PermissionService"/> class.
        /// </summary>
        /// <param name="authService">
        ///     The authorization service responsible for managing user permissions.
        /// </param>
        /// <param name="loggerService">
        ///     log service used for logging authorization breaches with audit logger service.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="authService"/> is null.
        /// </exception>
        public PermissionService(IAuthorizationService authService, ILoggerService loggerService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        }


        /// <summary>
        ///     Asynchronously validates the permissions of a user identified by the specified ID.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the user whose permissions are to be validated.
        /// </param>
        /// <returns>
        ///     A <see cref="PermissionServiceResult"/> indicating the outcome of the validation:
        ///     - If the user has the necessary permissions, returns a result with Success set to true.
        ///     - If the user lacks the required permissions, returns a result with Success set to false 
        ///       and an appropriate error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when id is null.
        /// </exception>
        public async Task<PermissionServiceResult> ValidatePermissions(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            // Use the auth service to check permissions
            bool hasPermission = await _authService.ValidatePermission(id);

            if (!hasPermission)
            {
                await _loggerService.LogAuthorizationBreach();
                return GenerateErrorResult(ErrorMessages.Authorization.Forbidden);
            }

            return GenerateSuccsesResult();
        }


        /// <summary>
        ///     Generates a permission service result representing an error, with success set to false.
        /// </summary>
        /// <param name="errorMessage">
        ///     The error message to include in the result.
        /// </param>
        /// <returns>
        ///     A permission service result indicating failure, with a list of error messages.
        /// </returns>
        private static PermissionServiceResult GenerateErrorResult(string errorMessage)
        {
            return new PermissionServiceResult
            {
                Success = false,
                Errors = new List<string> { errorMessage }
            };
        }


        /// <summary>
        ///     Generates a permission service result with success set to true.
        /// </summary>
        /// <returns>
        ///     A permission service result indicating success, with the generated token.
        /// </returns>
        private static PermissionServiceResult GenerateSuccsesResult()
        {
            return new PermissionServiceResult
            {
                Success = true,
            };
        }
    }
}
