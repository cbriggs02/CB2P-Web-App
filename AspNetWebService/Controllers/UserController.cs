using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using AspNetWebService.Helpers;
using Microsoft.AspNetCore.Mvc;
using AspNetWebService.Models;
using System.Security.Claims;
using AutoMapper;
using System.Text;
using AspNetWebService.Models.DataTransferObjectModels;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;

namespace AspNetWebService.Controllers
{
    /// <summary>
    ///     Controller handling User-related API operations.
    ///     @Author: Christian Briglio
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserController"/> class with the specified dependencies.
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
        public UserController(SignInManager<User> signInManager, UserManager<User> userManager, ILogger<UserController> logger, IConfiguration configuration, IMapper mapper)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        ///     Retrieves all users from the database as DTOs.
        /// </summary>
        /// <param name="page">
        ///     The page of data to be returned.
        /// </param>
        /// <param name="pageSize">
        ///     The size of data to be returned per page.
        /// </param>
        /// <returns>
        ///     A list of user DTOs.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Gets a list of all users")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers(int page, int pageSize)
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

                if (users == null || users.Count == 0)
                {
                    return NotFound();
                }

                var userDTOs = users.Select(user => _mapper.Map<UserDTO>(user)).ToList();

                var paginationMetadata = new
                {
                    totalCount,
                    pageSize,
                    currentPage = page,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));

                return Ok(userDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching users.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        ///     Retrieves a specific user by ID from the database.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to retrieve.
        /// </param>
        /// <returns>
        ///     The specified user.
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Gets a user by id")]
        public async Task<ActionResult<UserDTO>> GetUserById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                ModelState.AddModelError(string.Empty, "ID parameter cannot be null or empty.");
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                var userDTO = _mapper.Map<UserDTO>(user);

                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user by ID.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        ///     Creates a new user based on the information provided in the UserDTO object.
        /// </summary>
        /// <param name="userDTO">
        ///     The UserDTO object containing user information.
        /// </param>
        /// <returns>
        ///     Returns a response indicating the creation status.
        ///     - If successful, returns a 201 Created response with the created user's details in a UserDTO format.
        ///     - If the userDTO is null or invalid, returns a 400 Bad Request response with appropriate error details.
        ///     - If the provided username or email already exists, returns a 400 Bad Request response indicating the issue.
        ///     - If an error occurs during user creation, returns a 500 Internal Server Error response.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Creates a new user")]
        public async Task<ActionResult<User>> CreateUser([FromBody] UserDTO userDTO)
        {
            if (userDTO == null)
            {
                ModelState.AddModelError(string.Empty, "User parameter cannot be null or empty.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await UserNameExists(userDTO.UserName))
            {
                ModelState.AddModelError(string.Empty, "The provided username must be unique");
                return BadRequest(ModelState);
            }

            if (await EmailExists(userDTO.Email))
            {
                ModelState.AddModelError(string.Empty, "The provided email is already being used");
                return BadRequest(ModelState);
            }

            return await CreateUserAsync(userDTO);
        }

        /// <summary>
        ///     Creates a new User entity based on the information provided in the UserDTO object and handles the user creation process.
        /// </summary>
        /// <param name="userDTO">
        ///     The UserDTO object containing user information.
        /// </param>
        /// <returns>
        ///     Returns a response indicating the user creation status.
        ///     - If the user is successfully created, returns a response with a 201 Created status along with the created user's details in a UserDTO format.
        ///     - If an error occurs during user creation, returns a 500 Internal Server Error response with an appropriate message.
        /// </returns>
        private async Task<ActionResult<User>> CreateUserAsync(UserDTO userDTO)
        {
            if (userDTO == null)
            {
                ModelState.AddModelError(string.Empty, "UserDTO parameter cannot be null.");
                return BadRequest(ModelState);
            }

            try
            {
                // Create a new User entity with UserDTO data
                var newUser = new User
                {
                    UserName = userDTO.UserName,
                    FirstName = userDTO.FirstName,
                    LastName = userDTO.LastName,
                    Email = userDTO.Email,
                    PhoneNumber = userDTO.PhoneNumber,
                    Country = userDTO.Country,
                    LockoutEnd = DateTimeOffset.UtcNow
                };

                // Handle the user creation process
                return await HandleUserCreation(userDTO, newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user with UserManager.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating user.");
            }
        }

        /// <summary>
        ///     Handles the creation of a new User entity with the provided UserDTO data using UserManager.
        /// </summary>
        /// <param name="userDTO">
        ///     The UserDTO object containing user information.
        /// </param>
        /// <param name="newUser">
        ///     The newly created User entity.
        /// </param>
        /// <returns>
        ///     Returns a response indicating the user creation status.
        ///     - If the user is successfully created, returns a response with a 201 Created status along with the created user's details in a UserDTO format.
        ///     - If there are errors during user creation, returns a 400 Bad Request status with error details in the ModelState.
        /// </returns>
        private async Task<ActionResult<User>> HandleUserCreation(UserDTO userDTO, User newUser)
        {
            if (userDTO == null || newUser == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid input parameters.");
                return BadRequest(ModelState);
            }

            // Create the user without password
            var result = await _userManager.CreateAsync(newUser);

            if (result.Succeeded)
            {
                // Map the created User object back to a UserDTO
                var createdUserDTO = new UserDTO
                {
                    UserName = newUser.UserName,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    Email = newUser.Email,
                    PhoneNumber = newUser.PhoneNumber,
                    Country = newUser.Country
                };
                // User created successfully, return the UserDTO
                return CreatedAtAction(nameof(GetUserById), new { id = newUser.Id }, createdUserDTO);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        ///     Updates user information based on the provided user ID and UserDTO. This method is responsible for handling user updates,
        ///     excluding the password update functionality, which should be managed separately.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to update.
        /// </param>
        /// <param name="userDTO">
        ///     The UserDTO object containing updated user information.
        /// </param>
        /// <returns>
        ///     Returns a NoContent result if the update is successful.
        ///     Returns BadRequest if the user parameter or ID is null or empty, or if IDs do not match.
        ///     Returns NotFound if the user with the given ID is not found.
        ///     Returns BadRequest with model state errors if there are issues during the update process.
        ///     Returns StatusCode 500 if an unexpected error occurs.
        /// </returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Updates a user by id")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDTO userDTO)
        {
            if (string.IsNullOrWhiteSpace(id) || userDTO == null)
            {
                ModelState.AddModelError(string.Empty, "User parameter and ID cannot be null or empty.");
                return BadRequest(ModelState);
            }

            try
            {
                var existingUser = await _userManager.FindByIdAsync(id);

                if (existingUser == null)
                {
                    return NotFound();
                }

                if (id != existingUser.Id)
                {
                    ModelState.AddModelError(string.Empty, "IDs do not match.");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (await UserNameExists(userDTO.UserName))
                {
                    ModelState.AddModelError(string.Empty, "The provided username must be unique");
                    return BadRequest(ModelState);
                }

                if (await EmailExists(userDTO.Email))
                {
                    ModelState.AddModelError(string.Empty, "The provided email is already being used");
                    return BadRequest(ModelState);
                }

                return await UpdateUserAsync(userDTO, existingUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        ///     Updates the information of an existing user based on the provided UserDTO and existing user entity.
        /// </summary>
        /// <param name="userDTO">
        ///     The UserDTO object containing updated user information.
        /// </param>
        /// <param name="existingUser">
        ///     The existing User entity to be updated.
        /// </param>
        /// <returns>
        ///     Returns a NoContent result if the update is successful.
        ///     Returns BadRequest if there are issues with the provided user object or update process.
        /// </returns>
        private async Task<IActionResult> UpdateUserAsync(UserDTO userDTO, User existingUser)
        {
            // Update existing User with UserDTO data
            existingUser.UserName = userDTO.UserName;
            existingUser.FirstName = userDTO.FirstName;
            existingUser.LastName = userDTO.LastName;
            existingUser.Email = userDTO.Email;
            existingUser.PhoneNumber = userDTO.PhoneNumber;
            existingUser.Country = userDTO.Country;

            var result = await _userManager.UpdateAsync(existingUser);

            if (result.Succeeded)
            {
                return NoContent();
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        ///     Deletes a user by ID.
        /// </summary>
        ///     <param name="id">
        /// The ID of the user to delete.
        /// </param>
        /// <returns>
        ///     An IActionResult representing the result of the operation.
        /// </returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Deletes a user by id")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                ModelState.AddModelError(string.Empty, "Id cannot be null or empty.");
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    return Ok();
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        ///     Updates the password for a user identified by the specified ID.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password will be updated.
        /// </param>
        /// <param name="password">
        ///     The new password to be set.
        /// </param>
        /// <param name="passwordConfirmed">
        ///     Confirmation of the new password.
        /// </param>
        /// <returns>
        ///     Returns an IActionResult indicating the result of the password update operation:
        ///     - 200 OK if the password has been successfully updated.
        ///     - 404 Not Found if the user with the given ID is not found.
        ///     - 400 Bad Request if the provided parameters are null or empty or if passwords do not match.
        ///     - 500 Internal Server Error if an unexpected error occurs during processing.
        /// </returns>
        [HttpPut("setPassword/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Sets a password for a user by id")]
        public async Task<IActionResult> SetPassword(string id, [FromBody] string password, string passwordConfirmed)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordConfirmed))
            {
                ModelState.AddModelError(string.Empty, "Parameters cannot be null or empty.");
                return BadRequest(ModelState);
            }

            if (!passwordConfirmed.Equals(password))
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match");
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                var result = await _userManager.AddPasswordAsync(user, password);

                if (result.Succeeded)
                {
                    return Ok("Password has been created");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        ///     Updates the password for a user identified by the specified ID.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password will be updated.
        /// </param>
        /// <param name="currentPassword">
        ///     The current password for authentication.
        /// </param>
        /// <param name="newPassword">
        ///     The new password to be set.
        /// </param>
        /// <returns>
        ///     Returns an IActionResult indicating the result of the password update operation:
        ///     - 200 OK if the password has been successfully updated.
        ///     - 404 Not Found if the user with the given ID is not found.
        ///     - 400 Bad Request if the provided parameters are null or empty or if the current password is incorrect.
        ///     - 500 Internal Server Error if an unexpected error occurs during processing.
        /// </returns>
        [HttpPut("updatePassword/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Updates a password for a user by id")]
        public async Task<IActionResult> UpdatePassword(string id, [FromBody] string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError(string.Empty, "Parameters cannot be null or empty.");
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                // Verify the current password against the stored hash
                var passwordIsValid = await _userManager.CheckPasswordAsync(user, currentPassword);

                if (!passwordIsValid)
                {
                    ModelState.AddModelError(string.Empty, "Incorrect password");
                    return BadRequest(ModelState);
                }

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

                if (result.Succeeded)
                {
                    return Ok("Password has been updated");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating password.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        ///     Activates a user by updating the AccountStatus field to 1.
        /// </summary>
        /// <param name="id">
        ///     The user's identifier.
        /// </param>
        /// <returns>
        ///     - 200 OK if the activation is successful.
        ///     - 404 NotFound if the user with the provided id is not found.
        ///     - 400 BadRequest if the id parameter is null or empty, or if the user is already activated.
        ///     - 500 InternalServerError if an unexpected error occurs during the activation process.
        /// </returns>
        [HttpPut("activateUser/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Activates a user by id")]
        public async Task<IActionResult> ActivateUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                ModelState.AddModelError(string.Empty, "Id Parameter cannot be null or empty.");
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                if (user.AccountStatus == 1)
                {
                    ModelState.AddModelError(string.Empty, "User has already been activated in the system.");
                    return BadRequest(ModelState);
                }

                user.AccountStatus = 1;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                return Ok("User activated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while activating user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        ///     Deactivates a user by their unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to deactivate.
        /// </param>
        /// <returns>
        ///     An IActionResult representing the result of the deactivation operation.
        /// </returns>
        [HttpPut("deactivateUser/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Deactivates a user by id")]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                ModelState.AddModelError(string.Empty, "Id Parameter cannot be null or empty.");
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                if (user.AccountStatus == 0)
                {
                    ModelState.AddModelError(string.Empty, "User has not been activated in the system.");
                    return BadRequest(ModelState);
                }

                user.AccountStatus = 0;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                return Ok("User deactivated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deactivating user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        ///     Logs in a user by validating the provided credentials against the user database.
        /// </summary>
        /// <param name="model">
        ///     The <see cref="Login"/> model containing user login credentials.
        /// </param>
        /// <returns>
        ///     Returns an action result:
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the login is successful.
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the request body is invalid or the login attempt is unsuccessful.
        ///     - <see cref="StatusCodes.Status500InternalServerError"/> (Internal Server Error) if an unexpected error occurs during the login process.
        /// </returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Logs in a user")]
        public async Task<ActionResult<User>> Login([FromBody] Login model)
        {
            if (model == null)
            {
                return BadRequest(new { Message = "Invalid request body. Please provide a valid Login model." });
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);

                if (user != null)
                {
                    if (user.AccountStatus == 0)
                    {
                        return BadRequest(new { Message = "This users account has not been activated inside the system yet." });
                    }

                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                    if (result.Succeeded)
                    {
                        // Generate JWT token
                        var token = GenerateJwtToken(user);
                        return Ok(new { Token = token });
                    }

                    return Unauthorized();
                }
            }
            return BadRequest(new { Message = "Invalid login attempt" });
        }

        /// <summary>
        ///     Generates a JWT token for the specified user based on configured settings.
        /// </summary>
        /// <param name="user">
        ///     The user for whom the token is generated.
        /// </param>
        /// <returns>
        ///     A string representing the generated JWT token.
        /// </returns>
        private string GenerateJwtToken(User user)
        {
            // Read JwtSettings from appsettings.json
            var validIssuer = _configuration["JwtSettings:ValidIssuer"];
            var validAudience = _configuration["JwtSettings:ValidAudience"];

            // Use the SecretKeyGenerator to generate a secret key dynamically
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKeyGenerator.GenerateRandomSecretKey(32)));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName),
            };

            var tokenOptions = new JwtSecurityToken(
                issuer: validIssuer,
                audience: validAudience,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromHours(1)),
                signingCredentials: signingCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return tokenString;
        }

        /// <summary>
        ///     Checks the existence of a user based on the provided ID asynchronously.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to check.
        /// </param>
        /// <returns>
        ///     Returns a boolean value indicating whether a user with the specified ID exists.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the provided ID is null or empty.
        /// </exception>
        private async Task<bool> UserExists(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id), "ID cannot be null.");
            }
            // Check if a user with the provided id exists using UserManager
            return await _userManager.FindByIdAsync(id) != null;
        }

        /// <summary>
        ///     Checks if a user with the provided username exists in the database.
        /// </summary>
        /// <param name="userName">
        ///     The username to check for existence.
        /// </param>
        /// <returns>
        ///     True if a user with the specified username exists, otherwise false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the provided userName is null or empty.
        /// </exception>
        private async Task<bool> UserNameExists(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException(nameof(userName), "User Name cannot be null.");
            }
            // Check if a user with the provided user name exists using UserManager
            return await _userManager.FindByNameAsync(userName) != null;
        }

        /// <summary>
        ///     Checks if a user with the provided email address exists in the database.
        /// </summary>
        /// <param name="email">
        ///     The email address to check for existence.
        /// </param>
        /// <returns>
        ///     True if a user with the specified email address exists, otherwise false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the provided email is null or empty.
        /// </exception>
        private async Task<bool> EmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentNullException(nameof(email), "Email cannot be null.");
            }
            // Check if a user with the provided email exists using UserManager
            return await _userManager.FindByEmailAsync(email) != null;
        }
    }
}


