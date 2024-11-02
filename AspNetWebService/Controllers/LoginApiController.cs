using AspNetWebService.Constants;
using AspNetWebService.Interfaces.Authentication;
using AspNetWebService.Models.ApiResponseModels.CommonApiResponses;
using AspNetWebService.Models.ApiResponseModels.LoginApiResponses;
using AspNetWebService.Models.RequestModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AspNetWebService.Controllers
{
    /// <summary>
    ///     Controller for handling API operations related to user authentication.
    ///     This controller processes all incoming requests associated with login management 
    ///     and delegates these requests to the login service, which encapsulates the business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class LoginApiController : ControllerBase
    {
        private readonly ILoginService _loginService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoginApiController"/> class with the specified dependencies.
        /// </summary>
        /// <param name="loginService">
        ///     Login service used for all login-related operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public LoginApiController(ILoginService loginService)
        {
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        }


        /// <summary>
        ///     Asynchronously processes all requests for logging in a user using their credentials.
        ///     This method delegates the login process to the required service.
        /// </summary>
        /// <param name="model">
        ///     The <see cref="LoginRequest"/> model containing the user's login credentials.
        /// </param>
        /// <returns>
        ///     Returns an action result:
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) with a JWT token if the login is successful.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the login service that occurred during the login attempt.
        ///       
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [AllowAnonymous]
        [HttpPost("tokens")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginApiResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.LoginApi.Login)]
        public async Task<ActionResult<LoginApiResponse>> Login([FromBody] LoginRequest model)
        {
            var result = await _loginService.Login(model);

            if (result.Success)
            {
                return Ok(new LoginApiResponse { Token = result.Token });
            }
            else
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorApiResponse {  Errors = result.Errors });
            }
        }
    }
}
