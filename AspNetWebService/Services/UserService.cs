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
        ///     Retrieves users with pagination metadata.
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
        public async Task<UserListResult> GetUsersAsync(int page, int pageSize)
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
        ///     Retrieves users from database who matches provided id.
        /// </summary>
        /// <param name="id">
        ///     Id of user to retrieve in system.
        /// </param>
        /// <returns>
        ///     User DTO repersentation of User who matches provided id.
        /// </returns>
        public async Task<UserResult> GetUserAsync(string id)
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
    }
}
