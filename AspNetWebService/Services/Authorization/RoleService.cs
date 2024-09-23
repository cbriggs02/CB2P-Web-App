using AspNetWebService.Constants;
using AspNetWebService.Interfaces.Authorization;
using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.Entities;
using AspNetWebService.Models.ServiceResultModels.RoleServiceResults;
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="RoleService"/> class.
        /// </summary>
        /// <param name="roleManager">
        ///     The role manager for handling role operations within the system.
        /// </param>
        /// <param name="userManager">
        ///     The user manager for handling user operations within the system.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the parameters are null.
        /// </exception>
        public RoleService(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
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
                .Select(x => new RoleDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                })
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
        ///     A task representing the asynchronous operation, returning a <see cref="RoleServiceResult"/>
        ///     indicating the creation status:
        ///     - If successful, returns a result with Success set to true.
        ///     - If the role already exists, returns an error message.
        ///     - If an error occurs during creation, returns a result with an error message.
        /// </returns>
        public async Task<RoleServiceResult> CreateRole(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.Role.AlreadyExist }
                };
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (result.Succeeded)
            {
                return new RoleServiceResult
                {
                    Success = true,
                };
            }
            else
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }


        /// <summary>
        ///     Asynchronously deletes a role from the system based on its unique ID.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the role to delete.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation, returning a <see cref="RoleServiceResult"/>
        ///     indicating the deletion status:
        ///     - If successful, returns a result with Success set to true.
        ///     - If the role is not found, returns an error message.
        ///     - If an error occurs during deletion, returns a result with an error message.
        /// </returns>
        public async Task<RoleServiceResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.Role.NotFound }
                };
            }

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return new RoleServiceResult
                {
                    Success = true,
                };
            }
            else
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
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
        ///     A task representing the asynchronous operation, returning a <see cref="RoleServiceResult"/>.
        ///     The result indicates the assignment status:
        ///     - If successful, Success is true.
        ///     - If the user ID or role name is invalid, an error message is returned.
        ///     - If the user already has the role, an error is returned.
        ///     - If an error occurs during the assignment, an error message is returned.
        /// </returns>
        public async Task<RoleServiceResult> AssignRole(string id, string roleName)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.User.NotFound }
                };
            }

            if (!IsUserActive(user))
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.Role.InactiveUser }
                };
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.Role.InvalidRole }
                };
            }

            if (await HasRole(user, roleName))
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.Role.HasRole }
                };
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                return new RoleServiceResult
                {
                    Success = true,
                };
            }
            else
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
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
        ///     A task representing the asynchronous operation, returning a <see cref="RoleServiceResult"/>
        ///     indicating the removal status:
        ///     - If successful, returns a result with Success set to true.
        ///     - If the user ID or role name is invalid, returns an error message.
        ///     - If an error occurs during removal, returns a result with an error message.
        /// </returns>
        public async Task<RoleServiceResult> RemoveRole(string id, string roleName)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.User.NotFound }
                };
            }

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.Role.InvalidRole }
                };
            }

            if (!await HasRole(user, roleName))
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.Role.MissingRole }
                };
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                return new RoleServiceResult
                {
                    Success = true,
                };
            }
            else
            {
                return new RoleServiceResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
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
