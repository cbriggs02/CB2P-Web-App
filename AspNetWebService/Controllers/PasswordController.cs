using AspNetWebService.Interfaces;
using AspNetWebService.Models.Request_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AspNetWebService.Controllers
{
    /// <summary>
    ///     Controller for processing Password-related API operations.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordService _passwordService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordController"/> class with the specified dependencies.
        /// </summary>
        /// <param name="passwordService">
        ///     Password service used for all password-related operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public PasswordController(IPasswordService passwordService)
        {
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }


        /// <summary>
        ///     Processes all requests for setting user password in system by id to required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password will be updated.
        /// </param>
        /// <param name="request">
        ///     A model object that contains information required for setting password, this includes the password and the confirmed password.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if setting the password was successful.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the set password attempt is unsuccessful or any parameters have not been provided.
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [AllowAnonymous]
        [HttpPut("setPassword/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Sets a password for a user by id in system.")]
        [Authorize]
        public async Task<IActionResult> SetPassword([FromRoute][Required] string id, [FromBody] SetPasswordRequest request)
        {
            var result = await _passwordService.SetPassword(id, request);

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
        ///     Processes all requests for updating a user password in the system by id to required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password will be updated.
        /// </param>
        /// <param name="request">
        ///     A model object that contains information required for updating password, this includes current and new password.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if updating the password was successful.
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) if the password update attempt is unsuccessful or any parameters have not been provided.
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [Authorize]
        [HttpPut("updatePassword/{id}")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Updates a password for a user by id in system.")]
        public async Task<IActionResult> UpdatePassword([FromRoute][Required] string id, [FromBody] UpdatePasswordRequest request)
        {
            var result = await _passwordService.UpdatePassword(id, request);

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
