using AspNetWebService.Constants;
using AspNetWebService.Interfaces.Authorization;
using AspNetWebService.Interfaces.UserManagement;
using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.Entities;
using AspNetWebService.Models.PaginationModels;
using AspNetWebService.Models.RequestModels.UserRequests;
using AspNetWebService.Models.ServiceResultModels.UserServiceResults;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetWebService.Services.UserManagement
{
    /// <summary>
    ///     Service responsible for interacting with user-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IPasswordHistoryService _passwordHistoryService;
        private readonly IPermissionService _permissionService;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userManager">
        ///     The user manager responsible for handling user management operations.
        /// </param>
        /// <param name="passwordHistoryService">
        ///     Service for managing password history, such as removing old passwords after a user is deleted.
        /// </param>
        /// <param name="permissionService">
        ///     Service for validating and checking user permissions within the system.
        /// </param>
        /// <param name="mapper">
        ///     Object mapper for converting between entities and data transfer objects (DTOs).
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when any of the provided service parameters are null.
        /// </exception>
        public UserService(UserManager<User> userManager, IPasswordHistoryService passwordHistoryService, IPermissionService permissionService, IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _passwordHistoryService = passwordHistoryService ?? throw new ArgumentNullException(nameof(passwordHistoryService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        /// <summary>
        ///     Asynchronously retrieves a paginated list of users from the database based on the request parameters.
        /// </summary>
        /// <param name="request">
        ///     The request object containing pagination details such as the page number, page size
        ///     and account status for optional filtering.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserServiceListResult"/>
        ///     containing the list of users and associated pagination metadata.
        /// </returns>
        public async Task<UserServiceListResult> GetUsers(UserListRequest request)
        {
            var query = _userManager.Users.AsQueryable();

            if (request.AccountStatus.HasValue)
            {
                query = query.Where(user => user.AccountStatus == request.AccountStatus.Value);
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(user => user.LastName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var userDTOs = users.Select(user => _mapper.Map<UserDTO>(user)).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            PaginationMetadata paginationMetadata = new()
            {
                TotalCount = totalCount,
                PageSize = request.PageSize,
                CurrentPage = request.Page,
                TotalPages = totalPages
            };

            return new UserServiceListResult { Users = userDTOs, PaginationMetadata = paginationMetadata };
        }


        /// <summary>
        ///     Asynchronously retrieves a specific user from the database by their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to retrieve.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserServiceResult"/>
        ///     with the user's data if found.
        /// </returns>
        public async Task<UserServiceResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.User.NotFound }
                };
            }

            var permissionResult = await _permissionService.ValidatePermissions(id);

            if (!permissionResult.Success)
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = permissionResult.Errors
                };
            }

            var userDTO = _mapper.Map<UserDTO>(user);

            return new UserServiceResult { User = userDTO };
        }


        /// <summary>
        ///     Asynchronously creates a new user in the system and stores their details in the database.
        /// </summary>
        /// <param name="user">
        ///     A <see cref="UserDTO"/> object containing the user's details.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserServiceResult"/>
        ///     indicating whether the user creation was successful.
        ///     - If successful, returns the details of the created user.
        ///     - If the user already exists or an error occurs, returns relevant error messages.
        /// </returns>
        public async Task<UserServiceResult> CreateUser(UserDTO user)
        {
            var newUser = new User
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Country = user.Country,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(newUser);

            if (result.Succeeded)
            {
                var returnObject = new UserDTO
                {
                    UserName = newUser.UserName,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    Email = newUser.Email,
                    PhoneNumber = newUser.PhoneNumber,
                    Country = newUser.Country
                };

                return new UserServiceResult
                {
                    User = returnObject,
                    Success = true
                };
            }
            else
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }


        /// <summary>
        ///     Asynchronously updates an existing user's details in the system based on their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to update.
        /// </param>
        /// <param name="user">
        ///     A <see cref="UserDTO"/> object containing the updated user details.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserServiceResult"/>
        ///     indicating the success or failure of the update operation.
        /// </returns>
        public async Task<UserServiceResult> UpdateUser(string id, UserDTO user)
        {
            var existingUser = await _userManager.FindByIdAsync(id);

            if (existingUser == null)
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.User.NotFound }
                };
            }

            var permissionResult = await _permissionService.ValidatePermissions(id);

            if (!permissionResult.Success)
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = permissionResult.Errors
                };
            }

            existingUser.UserName = user.UserName;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Country = user.Country;
            existingUser.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(existingUser);

            if (result.Succeeded)
            {
                return new UserServiceResult { Success = true };
            }
            else
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }


        /// <summary>
        ///     Asynchronously deletes a user from the system based on their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to delete.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserServiceResult"/>
        ///     indicating whether the deletion was successful.
        ///     - If successful, deletes associated password history as well.
        ///     - If the user is not found or an error occurs, returns relevant error messages.
        /// </returns>
        public async Task<UserServiceResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.User.NotFound }
                };
            }

            var permissionResult = await _permissionService.ValidatePermissions(id);

            if (!permissionResult.Success)
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = permissionResult.Errors
                };
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                // delete all stored passwords for user once user is deleted for data clean up.
                await _passwordHistoryService.DeletePasswordHistory(id);

                return new UserServiceResult
                {
                    Success = true,
                };
            }
            else
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }


        /// <summary>
        ///     Asynchronously activates a user account in the system based on their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to activate.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserServiceResult"/>
        ///     indicating the success or failure of the account activation.
        /// </returns>
        public async Task<UserServiceResult> ActivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.User.NotFound }
                };
            }

            var permissionResult = await _permissionService.ValidatePermissions(id);

            if (!permissionResult.Success)
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = permissionResult.Errors
                };
            }

            if (user.AccountStatus == 1)
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.User.AlreadyActivated }
                };
            }

            user.AccountStatus = 1;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new UserServiceResult
                {
                    Success = true
                };
            }
            else
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }


        /// <summary>
        ///     Asynchronously deactivates a user account in the system based on their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to deactivate.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation, returning a <see cref="UserServiceResult"/>
        ///     indicating the success or failure of the account deactivation.
        /// </returns>
        public async Task<UserServiceResult> DeactivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.User.NotFound }
                };
            }

            var permissionResult = await _permissionService.ValidatePermissions(id);

            if (!permissionResult.Success)
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = permissionResult.Errors
                };
            }

            if (user.AccountStatus == 0)
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = new List<string> { ErrorMessages.User.NotActivated }
                };
            }

            user.AccountStatus = 0;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return new UserServiceResult
                {
                    Success = true
                };
            }
            else
            {
                return new UserServiceResult
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
        }
    }
}
