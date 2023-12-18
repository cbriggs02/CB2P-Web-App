using AspNetWebService.Data;
using AspNetWebService.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetWebService.Controllers
{
    /// <summary>
    /// Controller handling User-related API operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserController> _logger;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the UserController class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="userManager">The UserManager for managing users.</param>
        /// <param name="logger">The logger instance for logging.</param>
        /// <param name="mapper">The IMapper instance for object mapping.</param>
        public UserController(ApplicationDbContext context, UserManager<User> userManager, ILogger<UserController> logger, IMapper mapper)
        {
            _context = context;
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
            var userDTOs = await _context.Users

              // Map users to DTO's (Data Transfer Objects) to limit exposed user information
              .Select(user => _mapper.Map<UserDTO>(user))
              .ToListAsync();

            return Ok(userDTOs);
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
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("ID parameter cannot be null or empty.");
            }

            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                // Create a DTO (Data Transfer Object) to limit exposed user information
                var userDTO = new UserDTO
                {
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                };

                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                // Log the exception for analysis
                _logger.LogError(ex, "Error occurred while fetching user by ID.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Creates a new user using the provided UserDTO object.
        /// </summary>
        /// <param name="userDTO">The UserDTO object containing user information.</param>
        /// <returns>
        /// Returns a response indicating the creation status.
        /// - If successful, returns a 201 Created response with the created user's details in a UserDTO format.
        /// - If the userDTO is invalid or user creation fails, returns a 400 Bad Request response with error details.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<User>> CreateUser(UserDTO userDTO)
        {
            if (userDTO == null)
            {
                return BadRequest("User parameter cannot be null or empty.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("The provided user object is invalid");
            }

            // Create new User with UserDTO data
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

            // Create the user with hashed password using UserManager
            var result = await _userManager.CreateAsync(newUser, userDTO.Password);

            if (result.Succeeded)
            {
                // Map the created User object back to a UserDTO
                var createdUserDTO = new UserDTO
                {
                    UserName = newUser.UserName,
                    Password = "",
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
                // User creation failed, handle errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates an existing user by ID.
        /// </summary>
        /// <param name="id">The ID of the user to update.</param>
        /// <param name="userDTO">The updated UserDTO object.</param>
        /// <returns>
        /// An IActionResult representing the result of the operation.
        /// - If the update is successful, returns a 204 NoContent response.
        /// - If the provided user object or ID is invalid, returns a 400 BadRequest response.
        /// - If the user to update is not found, returns a 404 NotFound response.
        /// </returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(string id, User user)
        {
            if (string.IsNullOrWhiteSpace(id) || user == null)
            {
                return BadRequest("user parameter and id cannot be null or empty.");
            }

            if (id != user.Id)
            {
                return BadRequest("Id URL parameter does not match the Id of the user object");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("The provided user object is invalid");
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
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
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("Id cannot be null or empty.");
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Checks if a user exists by ID.
        /// </summary>
        /// <param name="id">The ID of the user to check.</param>
        /// <returns>True if the user exists; otherwise, false.</returns>
        private bool UserExists(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id), "ID cannot be null.");
            }

            return _context.Users.Any(e => e.Id == id);
        }
    }
}


