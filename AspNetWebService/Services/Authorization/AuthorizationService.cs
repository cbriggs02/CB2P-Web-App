using AspNetWebService.Constants;
using AspNetWebService.Interfaces.Authentication;
using AspNetWebService.Interfaces.Authorization;
using AspNetWebService.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace AspNetWebService.Services.Authorization
{
    /// <summary>
    ///     Service responsible for interacting with authorization-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserContextService _userContextService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorizationService"/> class.
        /// </summary>
        /// <param name="userManager">
        ///     The user manager responsible for handling user management operations.
        ///     In this case used for locating users during permission validation.
        /// </param>
        /// <param name="userContextService">
        ///     The service used for accessing current HTTP context.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public AuthorizationService(UserManager<User> userManager, IUserContextService userContextService)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        }


        /// <summary>
        ///     Asynchronously validates permissions based on the current user's role and the target user's data:
        ///     - Regular users can only access their own data.
        ///     - Admin users can access their own data and any non-admin user's data,
        ///       but are restricted from accessing data of other admin users.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose permissions are being validated. 
        ///     This represents the target user whose data the current user is attempting to access.
        /// </param>
        /// <returns>
        ///     True if the current user has permission to access the target user's data; otherwise, false.
        ///     - Returns true if the current user is a regular user accessing their own data.
        ///     - Returns true if the current user is an admin accessing their own data or non-admin data.
        ///     - Returns false if an admin attempts to access another admin's data.
        /// </returns>
        public async Task<bool> ValidatePermission(string id)
        {
            var principal = _userContextService.GetClaimsPrincipal();
            var currentUserId = _userContextService.GetUserId(principal);
            var roles = _userContextService.GetRoles(principal);

            if (roles.Contains(Roles.Admin))
            {
                return await ValidateAdminPermission(id, currentUserId);
            }

            if(roles.Contains(Roles.SuperAdmin)) 
            {
                return true; // super admin can access any endpoint and data.
            }

            return IsSelfAccess(id, currentUserId);
        }


        /// <summary>
        ///     Determines if the current user is accessing their own data.
        /// </summary>
        /// <param name="userId">
        ///     The ID of the target user being accessed.
        /// </param>
        /// <param name="currentUserId">
        ///     The ID of the current authenticated user.
        /// </param>
        /// <returns>
        ///     True if the current user is accessing their own data; otherwise, false.
        /// </returns>
        private static bool IsSelfAccess(string userId, string currentUserId)
        {
            return userId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        ///     Asynchronously validates whether an admin user has permission to perform actions on another user.
        ///     Admin users can access any user except other admins.
        /// </summary>
        /// <param name="id">
        ///     The ID of the target user.
        /// </param>
        /// <param name="currentUserId">
        ///     The ID of the current admin user.
        /// </param>
        /// <returns>
        ///     True if the current user has permission; otherwise, false.
        /// </returns>
        private async Task<bool> ValidateAdminPermission(string id, string currentUserId)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return false;
            }

            if (await IsTargetSuperAdmin(user))
            {
                return false; // Can't access Super Admin
            }


            if (await IsTargetAdmin(user) && !IsSelfAccess(id, currentUserId))
            {
                return false; // Can't access another admin's data or super admin data
            }

            return true;
        }


        /// <summary>
        ///     Asynchronously checks if the target user is an admin.
        /// </summary>
        /// <param name="user">
        ///     The target user whose roles are being checked.
        /// </param>
        /// <returns>
        ///     True if the target user is an admin; otherwise, false.
        /// </returns>
        private async Task<bool> IsTargetAdmin(User user)
        {
            var targetUserRoles = await _userManager.GetRolesAsync(user);
            return targetUserRoles.Any(role => role == Roles.Admin);
        }


        /// <summary>
        ///     Asynchronously checks if the target user is an super admin.
        /// </summary>
        /// <param name="user">
        ///     The target user whose roles are being checked.
        /// </param>
        /// <returns>
        ///     True if the target user is an super admin; otherwise, false.
        /// </returns>
        private async Task<bool> IsTargetSuperAdmin(User user)
        {
            var targetUserRoles = await _userManager.GetRolesAsync(user);
            return targetUserRoles.Any(role => role == Roles.SuperAdmin);
        }
    }
}
