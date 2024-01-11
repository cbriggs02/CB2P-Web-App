using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AspNetWebService.Models;
using AutoMapper;

namespace AspNetWebService.Controllers
{
    /// <summary>
    /// Controller handling User-related API operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserController> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the UserController class.
        /// </summary>
        /// <param name="userManager">The UserManager for managing users.</param>
        /// <param name="logger">The logger instance for logging.</param>
        /// <param name="mapper">The IMapper instance for object mapping.</param>
        public UserController(UserManager<User> userManager, ILogger<UserController> logger, IMapper mapper)
        {
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all users from the database as DTOs.
        /// </summary>
        /// <returns>A list of user DTOs.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userDTOs = users.Select(user => _mapper.Map<UserDTO>(user)).ToList();

            return Ok(userDTOs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> Login(Login user)
        {
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "user login cannot be null or empty.");
                return BadRequest(ModelState);
            }

            try
            {
                //var user = await _userManager.FindByNameAsync(u);

                //if (user == null)
                //{
                //    return NotFound();
                //}

                // google how to handle logins using the user manager

               // var login = await _userManager.AddLoginAsync(user, )

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user by User name.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Retrieves a specific user by ID from the database.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <returns>The specified user.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<User>> GetUserById(string id)
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
        /// Creates a new user based on the information provided in the UserDTO object.
        /// </summary>
        /// <param name="userDTO">The UserDTO object containing user information.</param>
        /// <returns>
        /// Returns a response indicating the creation status.
        /// - If successful, returns a 201 Created response with the created user's details in a UserDTO format.
        /// - If the userDTO is null or invalid, returns a 400 Bad Request response with appropriate error details.
        /// - If the provided username or email already exists, returns a 400 Bad Request response indicating the issue.
        /// - If an error occurs during user creation, returns a 500 Internal Server Error response.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<User>> CreateUser(UserDTO userDTO)
        {
            if (userDTO == null)
            {
                ModelState.AddModelError(string.Empty, "User parameter cannot be null or empty.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "The provided user object is invalid");
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
        /// Creates a new User entity based on the information provided in the UserDTO object and handles the user creation process.
        /// </summary>
        /// <param name="userDTO">The UserDTO object containing user information.</param>
        /// <returns>
        /// Returns a response indicating the user creation status.
        /// - If the user is successfully created, returns a response with a 201 Created status along with the created user's details in a UserDTO format.
        /// - If an error occurs during user creation, returns a 500 Internal Server Error response with an appropriate message.
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
                    BirthDate = userDTO.BirthDate,
                    Email = userDTO.Email,
                    PhoneNumber = userDTO.PhoneNumber,
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
        /// Handles the creation of a new User entity with the provided UserDTO data using UserManager.
        /// </summary>
        /// <param name="userDTO">The UserDTO object containing user information.</param>
        /// <param name="newUser">The newly created User entity.</param>
        /// <returns>
        /// Returns a response indicating the user creation status.
        /// - If the user is successfully created, returns a response with a 201 Created status along with the created user's details in a UserDTO format.
        /// - If there are errors during user creation, returns a 400 Bad Request status with error details in the ModelState.
        /// </returns>
        private async Task<ActionResult<User>> HandleUserCreation(UserDTO userDTO, User newUser)
        {
            if (userDTO == null || newUser == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid input parameters.");
                return BadRequest(ModelState);
            }

            // Create the user with hashed password using UserManager
            var result = await _userManager.CreateAsync(newUser);

            if (result.Succeeded)
            {
                // Map the created User object back to a UserDTO
                var createdUserDTO = new UserDTO
                {
                    UserName = newUser.UserName,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    BirthDate = newUser.BirthDate,
                    Email = newUser.Email,
                    PhoneNumber = newUser.PhoneNumber
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
        /// Updates user information based on the provided user ID and UserDTO. This method is responsible for handling user updates,
        /// excluding the password update functionality, which should be managed separately.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="userDTO">The UserDTO object containing updated user information.</param>
        /// <returns>
        /// Returns a NoContent result if the update is successful.
        /// Returns BadRequest if the user parameter or ID is null or empty, or if IDs do not match.
        /// Returns NotFound if the user with the given ID is not found.
        /// Returns BadRequest with model state errors if there are issues during the update process.
        /// Returns StatusCode 500 if an unexpected error occurs.
        /// </returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(string id, UserDTO userDTO)
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
                    ModelState.AddModelError(string.Empty, "The provided user object is invalid");
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
        /// Updates the information of an existing user based on the provided UserDTO and existing user entity.
        /// </summary>
        /// <param name="userDTO">The UserDTO object containing updated user information.</param>
        /// <param name="existingUser">The existing User entity to be updated.</param>
        /// <returns>
        /// Returns a NoContent result if the update is successful.
        /// Returns BadRequest if there are issues with the provided user object or update process.
        /// </returns>
        private async Task<IActionResult> UpdateUserAsync(UserDTO userDTO, User existingUser)
        {
            // Update existing User with UserDTO data
            existingUser.UserName = userDTO.UserName;
            existingUser.FirstName = userDTO.FirstName;
            existingUser.LastName = userDTO.LastName;
            existingUser.Email = userDTO.Email;
            existingUser.PhoneNumber = userDTO.PhoneNumber;

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
        /// Deletes a user by ID.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>An IActionResult representing the result of the operation.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Updates the password for a user identified by the specified ID.
        /// </summary>
        /// <param name="id">The ID of the user whose password will be updated.</param>
        /// <param name="password">The new password to be set.</param>
        /// <param name="passwordConfirmed">Confirmation of the new password.</param>
        /// <returns>
        /// Returns an IActionResult indicating the result of the password update operation:
        /// - 200 OK if the password has been successfully updated.
        /// - 404 Not Found if the user with the given ID is not found.
        /// - 400 Bad Request if the provided parameters are null or empty or if passwords do not match.
        /// - 500 Internal Server Error if an unexpected error occurs during processing.
        /// </returns>
        [HttpPut("setPassword/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetPassword(string id, string password, string passwordConfirmed)
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
        /// Updates the password for a user identified by the specified ID.
        /// </summary>
        /// <param name="id">The ID of the user whose password will be updated.</param>
        /// <param name="currentPassword">The current password for authentication.</param>
        /// <param name="newPassword">The new password to be set.</param>
        /// <returns>
        /// Returns an IActionResult indicating the result of the password update operation:
        /// - 200 OK if the password has been successfully updated.
        /// - 404 Not Found if the user with the given ID is not found.
        /// - 400 Bad Request if the provided parameters are null or empty or if the current password is incorrect.
        /// - 500 Internal Server Error if an unexpected error occurs during processing.
        /// </returns>
        [HttpPut("updatePassword/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePassword(string id, string currentPassword, string newPassword)
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
        /// Checks the existence of a user based on the provided ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the user to check.</param>
        /// <returns>Returns a boolean value indicating whether a user with the specified ID exists.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided ID is null or empty.</exception>
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
        /// Checks if a user with the provided username exists in the database.
        /// </summary>
        /// <param name="userName">The username to check for existence.</param>
        /// <returns>True if a user with the specified username exists, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided userName is null or empty.</exception>
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
        /// Checks if a user with the provided email address exists in the database.
        /// </summary>
        /// <param name="email">The email address to check for existence.</param>
        /// <returns>True if a user with the specified email address exists, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided email is null or empty.</exception>
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


