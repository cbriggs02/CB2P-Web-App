using AspNetWebService.Interfaces;
using AspNetWebService.Models.Data_Transfer_Object_Models;
using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.Entities;
using AspNetWebService.Models.Request_Models.UserRequests;
using AspNetWebService.Models.Result_Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetWebService.Services
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
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructor for the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userManager">
        ///     The user manager used for managing user-related operations.
        /// </param>
        /// <param name="passwordHistoryService">
        ///     This service is used for deleting passwords in history for removed users.
        /// </param>
        /// <param name="mapper">
        ///     The mapper used for mapping objects between different types.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public UserService(UserManager<User> userManager, IPasswordHistoryService passwordHistoryService, IMapper mapper)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _passwordHistoryService = passwordHistoryService ?? throw new ArgumentNullException(nameof(passwordHistoryService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        /// <summary>
        ///     Retrieves all users from database by page number and page size.
        /// </summary>
        /// <param name="request">
        ///     A model containing information used in request, such as a page number and page size.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a UserResult object.
        /// </returns>
        public async Task<UserListResult> GetUsers(UserListRequest request)
        {
            var totalCount = await _userManager.Users.CountAsync();

            var users = await _userManager.Users
                .OrderBy(user => user.LastName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var userDTOs = users.Select(user => _mapper.Map<UserDTO>(user)).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            PaginationMetadata paginationMetadata = new PaginationMetadata
            {
                TotalCount = totalCount,
                PageSize = request.PageSize,
                CurrentPage = request.Page,
                TotalPages = totalPages
            };

            return new UserListResult { Users = userDTOs, PaginationMetadata = paginationMetadata };
        }


        /// <summary>
        ///    Retrieves a user from database based on provided user id.
        /// </summary>
        /// <param name="id">
        ///     Id of user to retrieve in system.
        /// </param>
        /// <returns>
        ///     User DTO representation of User who matches provided id.
        /// </returns>
        public async Task<UserResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            var userDTO = _mapper.Map<UserDTO>(user);

            return new UserResult { User = userDTO };
        }


        /// <summary>
        ///     Creates a user in the database.
        /// </summary>
        /// <param name="user">
        ///     The UserDTO object containing user information.
        /// </param>
        /// <returns>
        ///     Returns a UserResult object indicating the creation status.
        ///     - If successful, returns a UserResult with Success set to true and the created user's details.
        ///     - If the userDTO is null, returns a UserResult with Success set to false and an error message.
        ///     - If the provided username or email already exists, returns a UserResult with Success set to false and an error message.
        ///     - If an error occurs during user creation, returns a UserResult with Success set to false and an error message.
        /// </returns>
        public async Task<UserResult> CreateUser(UserDTO user)
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

                return new UserResult
                {
                    User = returnObject,
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


        /// <summary>
        ///     Updates a user in the database based on provided id and new information in model object.
        /// </summary>
        /// <param name="id">
        ///     Id used to locate user to be updated inside the system.
        /// </param>
        /// <param name="user">
        ///     user information required to change the located user model object inside the system.
        /// </param>
        /// <returns>
        ///     Returns a UserResult object indicating the update status.
        ///     - If successful, returns a UserResult with Success set to true.
        ///     - If the provided username or email already exists, returns a UserResult with Success set to false and an error message.
        ///     - If an error occurs during the update, returns a UserResult with Success set to false and an error message.
        /// </returns>
        public async Task<UserResult> UpdateUser(string id, UserDTO user)
        {
            var existingUser = await _userManager.FindByIdAsync(id);

            if (existingUser == null)
            {
                return new UserResult
                {
                    Success = false,
                    Errors = new List<string> { "User not found." }
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
                return new UserResult { Success = true };
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


        /// <summary>
        ///     Deletes a user in the database based on provided id.
        /// </summary>
        /// <param name="id">
        ///     Id used to represent the user to be located and deleted inside the system.
        /// </param>
        /// <returns>
        ///     Returns a UserResult Object indicating the delete status.
        ///     - If successful, returns a UserResult with success set to true.
        ///     - If the provided id could not be located in the system returns a error message.
        ///     - If an error occurs during deletion, returns UserResult with error message.
        /// </returns>
        public async Task<UserResult> DeleteUser(string id)
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

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                // delete all stored passwords for user once user is deleted for data clean up.
                await _passwordHistoryService.DeletePasswordHistory(id);

                return new UserResult
                {
                    Success = true,
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


        /// <summary>
        ///     Activates a user in database by changing flag based on provided id.
        /// </summary>
        /// <param name="id">
        ///     Id to identify user inside system to be activated during request.
        /// </param>
        /// <returns>
        ///     Returns a UserResult Object indicating the activate user status.
        ///     - If successful, returns a UserResult with success set to true.
        ///     - If the provided id could not be located in the system returns a error message.
        ///     - If the located user is already activated, returns a error message.
        ///     - If an error occurs during activation, returns UserResult with error message.
        /// </returns>
        public async Task<UserResult> ActivateUser(string id)
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

            if (user.AccountStatus == 1)
            {
                return new UserResult
                {
                    Success = false,
                    Errors = new List<string> { "User already activated inside the system." }
                };
            }

            user.AccountStatus = 1;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
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


        /// <summary>
        ///     Deactivates a user in database by changing flag based on provided id.
        /// </summary>
        /// <param name="id">
        ///     Id used to identify user who is being deactivated inside the system.
        /// </param>
        /// <returns>
        ///     Returns a UserResult Object indicating the deactivate user status.
        ///     - If successful, returns a UserResult with success set to true.
        ///     - If the provided id could not be located in the system returns a error message.
        ///     - If the located user is already deactivated, returns a error message.
        ///     - If an error occurs during deactivation, returns UserResult with error message.
        /// </returns>
        public async Task<UserResult> DeactivateUser(string id)
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

            if (user.AccountStatus == 0)
            {
                return new UserResult
                {
                    Success = false,
                    Errors = new List<string> { "User has not been activated in the system." }
                };
            }

            user.AccountStatus = 0;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
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
    }
}
