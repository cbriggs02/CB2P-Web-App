using AspNetWebService.Constants;
using AspNetWebService.Interfaces.Authorization;
using AspNetWebService.Interfaces.UserManagement;
using AspNetWebService.Interfaces.Utilities;
using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.Entities;
using AspNetWebService.Models.ServiceResultModels.Authorization;
using AspNetWebService.Models.ServiceResultModels.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetWebService.Services.Authorization
{
    /// <summary>
    ///     Service responsible for interacting with role-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class RoleService : IRoleService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly IParameterValidator _parameterValidator;
        private readonly IServiceResultFactory _serviceResultFactory;
        private readonly IUserLookupService _userLookupService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleService"/> class.
        /// </summary>
        /// <param name="roleManager">
        ///     The role manager for handling role operations within the system.
        /// </param>
        /// <param name="userManager">
        ///     The user manager for handling user operations within the system.
        /// </param>
        /// <param name="parameterValidator">
        ///     The paramter validator service used for defense checking service paramters.
        /// </param>
        /// <param name="serviceResultFactory">
        ///     The service used for creating the result objects being returned in operations.
        /// </param>
        /// <param name="userLookupService">'
        ///     The service used for looking up users in the system.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the parameters are null.
        /// </exception>
        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, IParameterValidator parameterValidator, IServiceResultFactory serviceResultFactory, IUserLookupService userLookupService)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
            _serviceResultFactory = serviceResultFactory ?? throw new ArgumentNullException(nameof(serviceResultFactory));
            _userLookupService = userLookupService ?? throw new ArgumentNullException(nameof(userLookupService));
        }


        /// <summary>
        ///     Asynchronously retrieves all roles from the database, ordered by name.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation that returns an <see cref=" RoleServiceListResult"/>
        ///     containing all roles in the system.
        /// </returns>
        public async Task<RoleServiceListResult> GetRoles()
        {
            var roles = await _roleManager.Roles
                .OrderBy(x => x.Name)
                .Select(x => new RoleDTO { Id = x.Id, Name = x.Name })
                .AsNoTracking()
                .ToListAsync();

            return new RoleServiceListResult { Roles = roles };
        }


        /// <summary>
        ///     Asynchronously creates a new role in the system with the specified name.
        /// </summary>
        /// <param name="roleName">
        ///     The name of the role to create.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation, returning a <see cref="ServiceResult"/>
        ///     indicating the creation status:
        ///     - If successful, returns a result with Success set to true.
        ///     - If the role already exists, returns an error message.
        ///     - If an error occurs during creation, returns a result with an error message.
        /// </returns>
        public async Task<ServiceResult> CreateRole(string roleName)
        {
            _parameterValidator.ValidateNotNullOrEmpty(roleName, nameof(roleName));

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Role.AlreadyExist });
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            return _serviceResultFactory.GeneralOperationSuccess();
        }


        /// <summary>
        ///     Asynchronously deletes a role from the system based on its unique ID.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the role to delete.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation, returning a <see cref="ServiceResult"/>
        ///     indicating the deletion status:
        ///     - If successful, returns a result with Success set to true.
        ///     - If the role is not found, returns an error message.
        ///     - If an error occurs during deletion, returns a result with an error message.
        /// </returns>
        public async Task<ServiceResult> DeleteRole(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Role.NotFound });
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            return _serviceResultFactory.GeneralOperationSuccess();
        }


        /// <summary>
        ///     Asynchronously assigns a specified role to a user identified by their unique ID.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the user to whom the role is being assigned.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role to assign to the user.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation, returning a <see cref="ServiceResult"/>.
        ///     The result indicates the assignment status:
        ///     - If successful, Success is true.
        ///     - If the user ID or role name is invalid, an error message is returned.
        ///     - If the user already has the role, an error is returned.
        ///     - If an error occurs during the assignment, an error message is returned.
        /// </returns>
        public async Task<ServiceResult> AssignRole(string id, string roleName)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            _parameterValidator.ValidateNotNullOrEmpty(roleName, nameof(roleName));

            var userLookupResult = await _userLookupService.FindUserById(id);

            if (!userLookupResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure(userLookupResult.Errors.ToArray());
            }

            var user = userLookupResult.UserFound;

            if (!IsUserActive(user))
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Role.InactiveUser });
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Role.InvalidRole });
            }

            if (await HasRole(user, roleName))
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Role.HasRole });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            return _serviceResultFactory.GeneralOperationSuccess();
        }


        /// <summary>
        ///     Asynchronously removes a specified role to a user identified by their unique ID.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the user to whom the role is being removed.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role to remove from the user.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation, returning a <see cref="ServiceResult"/>
        ///     indicating the removal status:
        ///     - If successful, returns a result with Success set to true.
        ///     - If the user ID or role name is invalid, returns an error message.
        ///     - If an error occurs during removal, returns a result with an error message.
        /// </returns>
        public async Task<ServiceResult> RemoveRole(string id, string roleName)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));
            _parameterValidator.ValidateNotNullOrEmpty(roleName, nameof(roleName));

            var userLookupResult = await _userLookupService.FindUserById(id);

            if (!userLookupResult.Success)
            {
                return _serviceResultFactory.GeneralOperationFailure(userLookupResult.Errors.ToArray());
            }

            var user = userLookupResult.UserFound;

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Role.InvalidRole });
            }

            if (!await HasRole(user, roleName))
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.Role.MissingRole });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                return _serviceResultFactory.GeneralOperationFailure(result.Errors.Select(e => e.Description).ToArray());
            }

            return _serviceResultFactory.GeneralOperationSuccess();
        }


        /// <summary>
        ///     Asynchronously verifies if the specified user is assigned to the given role.
        /// </summary>
        /// <param name="user">
        ///     The user whose roles are being checked.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role to check for.
        /// </param>
        /// <returns>
        ///     A boolean value indicating whether the user has the specified role.
        /// </returns>
        private async Task<bool> HasRole(User user, string roleName)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.Any(role => role == roleName);
        }


        /// <summary>
        ///     Determines if the specified user is active based on their account status.
        /// </summary>
        /// <param name="user">
        ///     The user whose account status is being checked.
        /// </param>
        /// <returns>
        ///     True if the user is active (i.e., AccountStatus is 1); otherwise, false.
        /// </returns>
        private static bool IsUserActive(User user)
        {
            return user.AccountStatus == 1;
        }
    }
}
