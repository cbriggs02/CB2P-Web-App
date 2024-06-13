using AspNetWebService.Controllers;
using AspNetWebService.Interfaces;
using AspNetWebService.Models;
using AspNetWebService.Models.Data_Transfer_Object_Models;
using AspNetWebService.Models.DataTransferObjectModels;
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
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructor for UserService class.
        /// </summary>
        /// <param name="signInManager">
        ///     The sign-in manager used for user authentication.
        /// </param>
        /// <param name="userManager">
        ///     The user manager used for managing user-related operations.
        /// </param>
        /// <param name="logger">
        ///     The logger used for logging in the user controller.
        /// </param>
        /// <param name="configuration">
        ///     The configuration used for accessing app settings, including JWT settings.
        /// </param>
        /// <param name="mapper">
        ///     The mapper used for mapping objects between different types.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public UserService(SignInManager<User> signInManager, UserManager<User> userManager, ILogger<UserController> logger, IConfiguration configuration, IMapper mapper)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="page">
        ///     The page number.
        /// </param>
        /// <param name="pageSize">
        ///     The size of data to be returned per page.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a UserResult object.
        /// </returns>
        public async Task<UserListResult> GetUsers(int page, int pageSize)
        {
            try
            {
                var totalCount = await _userManager.Users.CountAsync();

                var users = await _userManager.Users
                    .OrderBy(user => user.LastName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var userDTOs = users.Select(user => _mapper.Map<UserDTO>(user)).ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                PaginationMetadata paginationMetadata = new PaginationMetadata
                {
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return new UserListResult { Users = userDTOs, PaginationMetadata = paginationMetadata };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching users.");
                throw;
            }
        }

        /// <summary>
        ///    
        /// </summary>
        /// <param name="id">
        ///     Id of user to retrieve in system.
        /// </param>
        /// <returns>
        ///     User DTO repersentation of User who matches provided id.
        /// </returns>
        public async Task<UserResult> GetUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);

                var userDTO = _mapper.Map<UserDTO>(user);

                return new UserResult { User = userDTO };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while fetching user.");
                throw;
            }
        }

        /// <summary>
        ///     
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
            var validationResult = await UserValidation(user);

            if (!validationResult.Success)
            {
                return validationResult;
            }

            try
            {
                var newUser = new User
                {
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Country = user.Country,
                    LockoutEnd = DateTimeOffset.UtcNow
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating user.");
                return new UserResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while creating the user." }
                };
            }
        }

        /// <summary>
        ///     
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
            var validationResult = await UserValidation(user);

            if (!validationResult.Success)
            {
                return validationResult;
            }

            try
            {
                var existingUser = await _userManager.FindByIdAsync(id);

                if(existingUser == null)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user.");
                return new UserResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while updaiting the user." }
                };
            }
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="id">
        ///     Id used to repersent the user to be located and deleted inside the system.
        /// </param>
        /// <returns>
        ///     Returns a UserResult Object indicating the delete status.
        ///     - If successful, returns a UserResult with success set to true.
        ///     - If the provided id could not be located in the system returns a error message.
        ///     - If an error occurs during deletion, returns UserResult with error message.
        /// </returns>
        public async Task<UserResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if(user == null)
                {
                    return new UserResult
                    {
                        Success = false,
                        Errors = new List<string> { "User not found." }
                    };
                }

                var result = await _userManager.DeleteAsync(user);

                if(result.Succeeded)
                {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting user.");
                return new UserResult
                {
                    Success = false,
                    Errors = new List<string> { "An error occurred while deleting the user." }
                };
            }
        }

        /// <summary>
        ///     Recieves a user DTO model to be validated for duplicates inside database.
        /// </summary>
        /// <param name="user">
        ///     user model object to be validated for duplicates inside system.
        /// </param>
        /// <returns>
        ///     Returns a result indicating duplicates were found for either userName or email
        ///     or that the validation check was succesful.
        /// </returns>
        private async Task<UserResult> UserValidation(UserDTO user)
        {
            var result = new UserResult
            {
                Success = true,
                Errors = new List<string>()
            };

            if (await UserNameExists(user.UserName))
            {
                result.Success = false;
                result.Errors.Add("The provided username must be unique.");
            }

            if (await EmailExists(user.Email))
            {
                result.Success = false;
                result.Errors.Add("The provided email is already being used.");
            }

            return result;
        }

        /// <summary>
        ///     Checks if a username already exists in the system.
        /// </summary>
        /// <param name="userName">
        ///     The username to check for existence.
        /// </param>
        /// <returns>
        ///     Returns true if the username exists, otherwise false.
        /// </returns>
        private async Task<bool> UserNameExists(string userName)
        {
            return await _userManager.FindByNameAsync(userName) != null;
        }

        /// <summary>
        ///     Checks if an email already exists in the system.
        /// </summary>
        /// <param name="email">
        ///     The email to check for existence.
        /// </param>
        /// <returns>
        ///     Returns true if the email exists, otherwise false.
        /// </returns>
        private async Task<bool> EmailExists(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }
    }
}
