using AspNetWebService.Interfaces;
using AspNetWebService.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AspNetWebService.Controllers
{
    /// <summary>
    ///     Controller for processing Authentication-related API operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthController"/> class with the specified dependencies.
        /// </summary>
        /// <param name="authService">
        ///     Authentication service used for all authentication-related operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }


        /// <summary>
        ///     Processes all requests for logging in a user in the system using credentials to required service.
        /// </summary>
        /// <param name="model">
        ///     The <see cref="Login"/> model containing user login credentials.
        /// </param>
        /// <returns>
        ///     Returns an action result:
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the login is successful.
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the request body is invalid or the login attempt is unsuccessful.
        /// </returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Logs in a user in system.")]
        public async Task<ActionResult<User>> Login([FromBody] LoginRequest model)
        {
            if (model == null)
            {
                ModelState.AddModelError(string.Empty, "Login body cannot be null or empty.");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.Login(model);

            if (result.Success)
            {
                return Ok(new { Token = result.Token });
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
        ///     Processes all requests for logging out a user in the system to required service.
        /// </summary>
        /// <returns>
        ///     Returns an action result:
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the logout is successful.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the logout attempt is unsuccessful or no user is logged in.
        /// </returns>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Logs out a user inside system.")]
        public async Task<IActionResult> Logout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                ModelState.AddModelError(string.Empty, "No user is logged in.");
                return BadRequest(ModelState);
            }

            var result = await _authService.Logout();

            if (result.Success)
            {
                return Ok();
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
    }
}
