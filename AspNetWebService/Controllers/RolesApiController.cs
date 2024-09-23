using AspNetWebService.Constants;
using AspNetWebService.Interfaces.Authorization;
using AspNetWebService.Models.ApiResponseModels;
using AspNetWebService.Models.ApiResponseModels.RolesApiResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AspNetWebService.Controllers
{
    /// <summary>
    ///     Controller for handling API operations related to roles.
    ///     This controller processes all incoming requests related to role management and delegates
    ///     them to the role service, which implements the business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class RolesApiController : ControllerBase
    {
        private readonly IRoleService _roleService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RolesApiController"/> 
        ///     class with the specified dependencies.
        /// </summary>
        /// <param name="roleService">
        ///     roles service used for all role-related operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public RolesApiController(IRoleService roleService)
        {
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
        }


        /// <summary>
        ///     Asynchronously processes requests for retrieving a list of all roles in the system,
        ///     delegating the operation to the required service.
        /// </summary>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) with a list of identity roles.
        ///     
        ///     - <see cref="StatusCodes.Status204NoContent"/> (No Content) if no roles are found in the system.
        ///     
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who 
        ///         is not authenticated or does not have the required role.
        /// </returns>
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetRolesApiResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(Summary = ApiDocumentation.RolesApi.GetRoles)]
        public async Task<ActionResult<GetRolesApiResponse>> GetRoles()
        {
            var result = await _roleService.GetRoles();

            if (result.Roles == null || !result.Roles.Any())
            {
                return NoContent();
            }

            return Ok(new GetRolesApiResponse { Roles = result.Roles });
        }


        /// <summary>
        ///     Asynchronously processes requests for creating a new role in the system 
        ///     by delegating the operation to the required service.
        /// </summary>
        /// <param name="roleName">
        ///     The name of the role being created in the system.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the role creation was successful.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the role service that occurred during role creation.
        ///       
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made 
        ///         by a user who is not authenticated or does not have the required role.
        /// </returns>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(Summary = ApiDocumentation.RolesApi.CreateRole)]
        public async Task<IActionResult> CreateRole([FromBody][Required(ErrorMessage = "Role Name is required.")] string roleName)
        {
            var result = await _roleService.CreateRole(roleName);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                return BadRequest(new ErrorApiResponse { Errors = result.Errors });
            }
        }


        /// <summary>
        ///     Asynchronously processes requests for deleting a role in the system 
        ///     by delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the role being deleted in the system.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the role deletion was successful.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the role service that occurred during role deletion.
        ///       
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made 
        ///         by a user who is not authenticated or does not have the required role.
        ///     
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the role is not found.
        /// </returns>
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.RolesApi.DeleteRole)]
        public async Task<IActionResult> DeleteRole([FromRoute][Required] string id)
        {
            var result = await _roleService.DeleteRole(id);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.Role.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();

                }

                return BadRequest(new ErrorApiResponse { Errors = result.Errors });
            }
        }


        /// <summary>
        ///     Asynchronously processes requests for assigning a role to a user in the system
        ///     by delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to whom the role is being assigned.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role being assigned to the user.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the role assignment was successful.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the role service that occurred during the role assignment.
        ///         
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made 
        ///         by a user who is not authenticated or does not have the required role.
        ///     
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.RolesApi.AssignRole)]
        public async Task<IActionResult> AssignRole([FromRoute][Required] string id, [FromBody][Required(ErrorMessage = "Role Name is required.")] string roleName)
        {
            var result = await _roleService.AssignRole(id, roleName);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorApiResponse { Errors = result.Errors });
            }
        }


        /// <summary>
        ///     Asynchronously processes requests for removing a role from a user in the system
        ///     by delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user to whom the role is being removed.
        /// </param>
        /// <param name="roleName">
        ///     The name of the role being removed to the user.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the role removal was successful.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors 
        ///         returned by the role service that occurred when removing the role.
        ///         
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made 
        ///         by a user who is not authenticated or does not have the required role.
        ///     
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the user is not found.
        /// </returns>
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("users/{id}/roles/{roleName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.RolesApi.RemoveRole)]
        public async Task<IActionResult> RemoveRole([FromRoute][Required] string id, [FromRoute][Required] string roleName)
        {
            var result = await _roleService.RemoveRole(id, roleName);

            if (result.Success)
            {
                return Ok();
            }
            else
            {
                if (result.Errors.Any(error => error.Contains(ErrorMessages.User.NotFound, StringComparison.OrdinalIgnoreCase)))
                {
                    return NotFound();
                }

                return BadRequest(new ErrorApiResponse { Errors = result.Errors });
            }
        }
    }
}
