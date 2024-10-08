﻿using AspNetWebService.Constants;
using AspNetWebService.Interfaces.Authorization;
using AspNetWebService.Interfaces.UserManagement;
using AspNetWebService.Models.Entities;
using AspNetWebService.Models.RequestModels.PasswordHistoryRequests;
using AspNetWebService.Models.RequestModels.PasswordRequests;
using AspNetWebService.Models.ServiceResultModels.PasswordResults;
using Microsoft.AspNetCore.Identity;

namespace AspNetWebService.Services.UserManagement
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
        private readonly IPasswordHistoryService _historyService;
        private readonly IPermissionService _permissionService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordService"/> class.
        /// </summary>
        /// <param name="userManager">
        ///     The user manager used for managing user-related operations.
        /// </param>
        /// <param name="historyService">
        ///     The service responsible for managing password history.
        /// </param>
        /// <param name="permissionService">
        ///     The service used for validating user permissions.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the parameters are null.
        /// </exception>
        public PasswordService(UserManager<User> userManager, IPasswordHistoryService historyService, IPermissionService permissionService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        }


        /// <summary>
        ///     Asynchronously sets a password for a user in the database based on the provided ID.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password is being set.
        /// </param>
        /// <param name="request">
        ///     A model object containing the password and confirmed password.
        /// </param>
        /// <returns>
        ///     A <see cref="PasswordServiceResult"/> indicating the outcome of the password setting operation:
        ///     - If successful, Success is set to true.
        ///     - If the user ID cannot be found, an error message is provided.
        ///     - If an error occurs during setup, an error message is returned.
        /// </returns>
        public async Task<PasswordServiceResult> SetPassword(string id, SetPasswordRequest request)
        {
            if (!request.PasswordConfirmed.Equals(request.Password))
            {
                return new PasswordServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.Password.Mismatch }
                };
            }


            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return new PasswordServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.User.NotFound }
                };
            }

            if (user.PasswordHash != null)
            {
                return new PasswordServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.Password.AlreadySet }
                };
            }

            var result = await _userManager.AddPasswordAsync(user, request.Password);

            if (result.Succeeded)
            {
                await CreatePasswordHistory(user);

                return new PasswordServiceResult
                {
                    Success = true
                };
            }
            else
            {
                return new PasswordServiceResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }


        /// <summary>
        ///     Asynchronously updates the password for a user in the database based on the provided ID.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password is being updated.
        /// </param>
        /// <param name="request">
        ///     A model object containing the current password and the new password.
        /// </param>
        /// <returns>
        ///     A <see cref="PasswordServiceResult"/> indicating the outcome of the password update operation:
        ///     - If successful, Success is set to true.
        ///     - If the user ID cannot be found or the current password is invalid, an error message is provided.
        ///     - If an error occurs during the update, an error message is returned.
        /// </returns>
        public async Task<PasswordServiceResult> UpdatePassword(string id, UpdatePasswordRequest request)
        {
            var permissionResult = await _permissionService.ValidatePermissions(id);

            if (!permissionResult.Success)
            {
                return new PasswordServiceResult
                {
                    Success = false,
                    Errors = permissionResult.Errors
                };
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null || user.PasswordHash == null)
            {
                return new PasswordServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.Password.InvalidCredentials }
                };
            }

            var passwordIsValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);

            if (!passwordIsValid)
            {
                return new PasswordServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.Password.InvalidCredentials }
                };
            }

            // Send password to be checked against users history for re-use errors
            var isPasswordReused = await IsPasswordReused(user.Id, request.NewPassword);

            if (isPasswordReused)
            {
                return new PasswordServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.Password.CannotReuse }
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                await CreatePasswordHistory(user);

                return new PasswordServiceResult
                {
                    Success = true
                };
            }
            else
            {
                return new PasswordServiceResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }


        /// <summary>
        ///     Asynchronously records the current password of the user in the password history.
        /// </summary>
        /// <param name="user">
        ///     The user whose password history is being recorded. This object should contain the user's ID and the current password hash.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task completes when the password history record has
        ///     been successfully added to the database.
        /// </returns>
        private async Task CreatePasswordHistory(User user)
        {
            var passwordHistoryRequest = new StorePasswordHistoryRequest
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash,
            };

            await _historyService.AddPasswordHistory(passwordHistoryRequest);
        }


        /// <summary>
        ///     Asynchronously checks if the specified password has been used previously by the specified user.
        /// </summary>
        /// <param name="userId">
        ///     The ID of the user whose password history is being checked.
        /// </param>
        /// <param name="password">
        ///     The password to check against the user's password history.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation. The result is a boolean indicating whether the password
        ///     has been previously used:
        ///     - true if the password is found in the user's history, indicating potential re-use.
        ///     - false if the password is not found, indicating no re-use.
        /// </returns>
        private async Task<bool> IsPasswordReused(string userId, string password)
        {
            var request = new SearchPasswordHistoryRequest
            {
                UserId = userId,
                Password = password
            };

            return await _historyService.FindPasswordHash(request);
        }
    }
}
