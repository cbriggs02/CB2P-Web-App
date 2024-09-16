using Microsoft.AspNetCore.Mvc;
using AspNetWebService.Models.DataTransferObjectModels;
using Swashbuckle.AspNetCore.Annotations;
using Newtonsoft.Json;
using AspNetWebService.Interfaces;
using System.ComponentModel.DataAnnotations;
using AspNetWebService.Models.Request_Models.UserRequests;
using AspNetWebService.Models.Entities;
using Microsoft.AspNetCore.Authorization;

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
        /// <param name="request">
        ///     A model containing information used in request, such as a page number and page size.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if retrieving the list of users was successful 
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the list is empty.
        /// </returns>
        [Authorize]
        [HttpGet]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Gets a list of users using page number and size in system.")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers([FromQuery] UserListRequest request)
        {
            var result = await _userService.GetUsers(request);

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
        /// </returns>
        [Authorize]
        [HttpGet("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Gets a user by id in system.")]
        public async Task<ActionResult<UserDTO>> GetUserById([FromRoute][Required] string id)
        {
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
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if creating the user was successful.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the user creation attempt is unsuccessful or user object has not been provided.
        /// </returns>
        [AllowAnonymous]
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [SwaggerOperation(Summary = "Creates a new user in system.")]
        public async Task<ActionResult<UserDTO>> CreateUser([FromBody] UserDTO user)
        {
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
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if updating the user was successful.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the user update attempt is unsuccessful or any parameters are missing.
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [Authorize]
        [HttpPut("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Updates a user by id in system.")]
        public async Task<IActionResult> UpdateUser([FromRoute][Required] string id, [FromBody] UserDTO user)
        {
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
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the user deletion was successful.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the user deletion attempt is unsuccessful or id was not provided.
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [Authorize]
        [HttpDelete("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Deletes a user by id in system.")]
        public async Task<IActionResult> DeleteUser([FromRoute][Required] string id)
        {
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
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the activation was successful.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the activation attempt is unsuccessful or id is not provided.
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [Authorize]
        [HttpPut("activateUser/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Activates a user by id in system.")]
        public async Task<IActionResult> ActivateUser([FromRoute][Required] string id)
        {
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
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the deactivation was successful.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the deactivation attempt is unsuccessful or id is not provided.
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [Authorize]
        [HttpPut("deactivateUser/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Deactivates a user by id in system.")]
        public async Task<IActionResult> DeactivateUser([FromRoute][Required] string id)
        {
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


