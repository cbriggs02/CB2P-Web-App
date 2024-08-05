using Microsoft.AspNetCore.Mvc;
using AspNetWebService.Models;
using AspNetWebService.Models.DataTransferObjectModels;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using AspNetWebService.Interfaces;

namespace AspNetWebService.Controllers
{
    /// <summary>
    ///     Controller for processing User-related API operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserController"/> class with the specified dependencies.
        /// </summary>
        /// <param name="userService">
        ///     User service used for all user-related operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public UserController(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }


        /// <summary>
        ///     Process all requests for retrieving list of users in system by page number and size to required service.
        /// </summary>
        /// <param name="page">
        ///     The page of data to be returned.
        /// </param>
        /// <param name="pageSize">
        ///     The size of data to be returned per page.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if retrieving the list of users was successful 
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the list is empty.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Gets a list of users using page number and size in system.")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers(int page, int pageSize)
        {
            var result = await _userService.GetUsers(page, pageSize);

            if (result.Users == null || result.Users.Count == 0)
            {
                return NotFound();
            }

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(result.PaginationMetadata));

            return Ok(result.Users);
        }


        /// <summary>
        ///    Processes all requests for retrieving a user in the system by provided id to required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to retrieve.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if retrieving the user was successful 
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the user retrieval attempt is unsuccessful or the id was not provided.
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Gets a user by id in system.")]
        public async Task<ActionResult<UserDTO>> GetUserById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                ModelState.AddModelError(string.Empty, "ID parameter cannot be null or empty.");
                return BadRequest(ModelState);
            }

            var result = await _userService.GetUser(id);

            if (result.User == null)
            {
                return NotFound();
            }

            return Ok(result.User);
        }


        /// <summary>
        ///     Processes all requests for creating a new user in system to required service.
        /// </summary>
        /// <param name="user">
        ///     The UserDTO object containing user information.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if creating the user was successful 
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the user creation attempt is unsuccessful or user object has not been provided.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Creates a new user in system.")]
        public async Task<ActionResult<User>> CreateUser([FromBody] UserDTO user)
        {
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User parameter cannot be null or empty.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.CreateUser(user);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetUserById), new { id = result.User.UserName }, result.User);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return BadRequest(ModelState);
            }
        }


        /// <summary>
        ///     Processes all requests for updating a user in the system by id to required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to update.
        /// </param>
        /// <param name="user">
        ///     The UserDTO object containing updated user information.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if updating the user was successful 
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the user update attempt is unsuccessful or any parameters are missing.
        /// </returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Updates a user by id in system.")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserDTO user)
        {
            if (string.IsNullOrWhiteSpace(id) || user == null)
            {
                ModelState.AddModelError(string.Empty, "User parameter and ID cannot be null or empty.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.UpdateUser(id, user);

            if (result.Success)
            {
                return NoContent();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains("user not found", StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }


                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return BadRequest(ModelState);
            }
        }


        /// <summary>
        ///     Processes all requests for deleting a user in the system by id to required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to delete.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the user deletion was successful 
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the user deletion attempt is unsuccessful or id was not provided.
        /// </returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Deletes a user by id in system.")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                ModelState.AddModelError(string.Empty, "Id cannot be null or empty.");
                return BadRequest(ModelState);
            }

            var result = await _userService.DeleteUser(id);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains("user not found", StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return BadRequest(ModelState);
            }
        }


        /// <summary>
        ///     Processes all requests for activating a user in the system by id to required service.
        /// </summary>
        /// <param name="id">
        ///     The user's identifier.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the activation was successful 
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the activation attempt is unsuccessful or id is not provided.
        /// </returns>
        [HttpPut("activateUser/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Activates a user by id in system.")]
        public async Task<IActionResult> ActivateUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                ModelState.AddModelError(string.Empty, "Id Parameter cannot be null or empty.");
                return BadRequest(ModelState);
            }

            var result = await _userService.ActivateUser(id);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains("user not found", StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return BadRequest(ModelState);
            }
        }


        /// <summary>
        ///     Processes all requests for deactivating a user in the system by id to required service.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the user to deactivate.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the deactivation was successful 
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the deactivation attempt is unsuccessful or id is not provided..
        /// </returns>
        [HttpPut("deactivateUser/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Deactivates a user by id in system.")]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                ModelState.AddModelError(string.Empty, "Id Parameter cannot be null or empty.");
                return BadRequest(ModelState);
            }

            var result = await _userService.DeactivateUser(id);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains("user not found", StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return BadRequest(ModelState);
            }
        }
    }
}


