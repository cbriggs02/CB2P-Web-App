using AspNetWebService.Interfaces;
using AspNetWebService.Models;
using AspNetWebService.Models.Request_Models;
using AspNetWebService.Models.Result_Models;
using Microsoft.AspNetCore.Identity;

namespace AspNetWebService.Services
{
    /// <summary>
    ///     Service responsible for interacting with password-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class PasswordService : IPasswordService
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<PasswordService> _logger;
        private readonly IPasswordHistoryService _historyService;


        /// <summary>
        ///     Constructor for the <see cref="PasswordService"/> class.
        /// </summary>
        /// <param name="userManager">
        ///     The user manager used for managing user-related operations.
        /// </param>
        /// <param name="logger">
        ///     The logger used for logging in the password service.
        /// </param>
        /// <param name="historyService">
        ///     The service responsible for managing password history.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public PasswordService(UserManager<User> userManager, ILogger<PasswordService> logger, IPasswordHistoryService historyService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
        }


        /// <summary>
        ///     Sets a password for a user in database based on provided id.
        /// </summary>
        /// <param name="id">
        ///     Id of user who password is being set inside the system.
        /// </param>
        /// <param name="request">
        ///     A model object that contains information required for setting password, this includes the password and the confirmed password.
        /// </param>
        /// <returns>
        ///     Returns a UserResult Object indicating the set password status.
        ///     - If successful, returns a UserResult with success set to true.
        ///     - If the provided id could not be located in the system returns a error message.
        ///     - If an error occurs during setup, returns UserResult with error message.
        /// </returns>
        public async Task<UserResult> SetPassword(string id, SetPasswordRequest request)
        {
            if (!request.PasswordConfirmed.Equals(request.Password))
            {
                return new UserResult
                {
                    Success = false,
                    Errors = new List<string> { "Passwords do not match." }
                };
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return new UserResult
                    {
                        Success = false,
                        Errors = new List<string> { "User not found." }
                    };
                }

                if (user.PasswordHash != null)
                {
                    return new UserResult
                    {
                        Success = false,
                        Errors = new List<string> { "Password has already been set." }
                    };
                }

                var result = await _userManager.AddPasswordAsync(user, request.Password);

                if (result.Succeeded)
                {
                    await CreatePasswordHistory(user);

                    return new UserResult
                    {
                        Success = true
                    };
                }
                else
                {
                    return new UserResult
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting password.");
                return new UserResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while setting the password." }
                };
            }
        }


        /// <summary>
        ///     Updates a password for user in database based on provided id.
        /// </summary>
        /// <param name="id">
        ///     Id to identify user to update password for inside the system.
        /// </param>
        /// <param name="request">
        ///     A model object that contains information required for updating password, this includes current and new password.
        /// </param>
        /// <returns>
        ///     Returns a UserResult Object indicating the update password status.
        ///     - If successful, returns a UserResult with success set to true.
        ///     - If the provided id could not be located in the system returns a error message.
        ///     - If an error occurs during update, returns UserResult with error message.
        /// </returns>
        public async Task<UserResult> UpdatePassword(string id, UpdatePasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null || user.PasswordHash == null)
                {
                    return new UserResult
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid credentials." }
                    };
                }

                var passwordIsValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);

                if (!passwordIsValid)
                {
                    return new UserResult
                    {
                        Success = false,
                        Errors = new List<string> { "Invalid credentials." }
                    };
                }

                var newPasswordHash = HashPassword(request.NewPassword, user);

                var isPasswordReused = await IsPasswordReused(user.Id, newPasswordHash);

                if (isPasswordReused)
                {
                    return new UserResult
                    {
                        Success = false,
                        Errors = new List<string> { "Cannot re-use passwords. Please choose new password." }
                    };
                }

                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                if (result.Succeeded)
                {
                    await CreatePasswordHistory(user);

                    return new UserResult
                    {
                        Success = true
                    };
                }
                else
                {
                    return new UserResult
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating password.");
                return new UserResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while updating the password." }
                };
            }
        }


        /// <summary>
        ///     Receives appropriate user data used to encapsulate into request object sent to password history service,
        ///     which saves users provided password in history.
        /// </summary>
        /// <param name="user">
        ///     The user whose password history is being recorded. This object is expected to contain the user's ID and the
        ///     current password hash.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task will complete when the password history record has
        ///     been successfully added to the database.
        /// </returns>
        private async Task CreatePasswordHistory(User user)
        {
            var passwordHistoryRequest = new PasswordHistoryRequest
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash,
            };

            await _historyService.AddPasswordHistory(passwordHistoryRequest);
        }


        /// <summary>
        ///     Checks if the provided password hash has been used before by the specified user.
        /// </summary>
        /// <param name="userId">
        ///     The ID of the user whose password history is being checked.
        /// </param>
        /// <param name="newPasswordHash">
        ///     The hashed password to check against the user's password history.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result is a boolean value indicating
        ///     whether the provided password hash is found in the user's password history.
        ///     - <c>true</c> if the password hash is found in the user's history, indicating potential re-use.
        ///     - <c>false</c> if the password hash is not found in the user's history, indicating no re-use.
        /// </returns>
        /// <remarks>
        ///     This method creates a PasswordHistoryRequest object to encapsulate the user ID and the hashed 
        ///     password, then calls the _historyService to check if the hash already exists in the user's 
        ///     password history. The result is a boolean indicating whether the password has been previously used.
        /// </remarks>
        private async Task<bool> IsPasswordReused(string userId, string newPasswordHash)
        {
            var request = new PasswordHistoryRequest
            {
                UserId = userId,
                PasswordHash = newPasswordHash
            };

            return await _historyService.FindPasswordHash(request);
        }


        /// <summary>
        ///     Hashes the specified password using the PasswordHasher for the given user.
        /// </summary>
        /// <param name="password">
        ///     The plain-text password to be hashed.
        /// </param>
        /// <param name="user">
        ///     The user object used to provide context for the hashing operation. 
        ///     It can influence the hashing process depending on the implementation of the hasher.
        /// </param>
        /// <returns>
        ///     The hashed password as a string.
        /// </returns>
        /// <remarks>
        ///     This method uses ASP.NET Core's PasswordHasher to hash the password. It is important to note 
        ///     that the PasswordHasher requires a user object for hashing. In this case, the user object is 
        ///     passed to ensure that the password is hashed in a contextually relevant manner.
        /// </remarks>
        private static string HashPassword(string password, User user)
        {
            var passwordHasher = new PasswordHasher<User>();
            return passwordHasher.HashPassword(user, password);
        }
    }
}
