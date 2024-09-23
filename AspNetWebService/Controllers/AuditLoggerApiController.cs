using AspNetWebService.Constants;
using AspNetWebService.Interfaces.Logging;
using AspNetWebService.Models.ApiResponseModels;
using AspNetWebService.Models.ApiResponseModels.AuditLogsApiResponses;
using AspNetWebService.Models.RequestModels.AuditLogRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace AspNetWebService.Controllers
{
    /// <summary>
    ///     Controller for handling API operations related to audit logs.
    ///     This controller processes all incoming requests related to audit log management and delegates
    ///     them to the audit log service, which implements the business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLoggerApiController : ControllerBase
    {
        private readonly IAuditLoggerService _auditLogService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLoggerApiController"/> 
        ///     class with the specified dependencies.
        /// </summary>
        /// <param name="auditLogService">
        ///     audit log service used for all audit-log-related operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public AuditLoggerApiController(IAuditLoggerService auditLogService)
        {
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        }


        /// <summary>
        ///     Asynchronously processes requests for retrieving a list of all audit logs in the system,
        ///     delegating the operation to the required service.
        /// </summary>
        /// <param name="request">
        ///     A model containing pagination details, such as the page number and page size and audit action for optional filtering.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) with a list of audit logs and pagination .
        ///     
        ///     - <see cref="StatusCodes.Status204NoContent"/> (No Content) if no audit logs are found in the system.
        ///     
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who 
        ///         is not authenticated or does not have the required role.
        /// </returns>
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAuditLogsApiResponse))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(Summary = ApiDocumentation.AuditLogsApi.GetLogs)]
        public async Task<ActionResult<GetAuditLogsApiResponse>> GetLogs([FromQuery] AuditLogListRequest request)
        {
            var result = await _auditLogService.GetLogs(request);

            if (result.Logs == null || !result.Logs.Any())
            {
                return NoContent();
            }

            var response = new GetAuditLogsApiResponse
            {
                Logs = result.Logs,
                PaginationMetadata = result.PaginationMetadata
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(response.PaginationMetadata));

            return Ok(response);
        }


        /// <summary>
        ///     Asynchronously processes requests for deleting a audit logs in the system by the provided ID,
        ///     delegating the operation to the required service.
        /// </summary>
        /// <param name="id">
        ///     The ID of the audit log to delete.
        /// </param>
        /// <returns>
        ///     - <see cref="StatusCodes.Status200OK"/> (OK) if the audit log deletion was successful.
        ///     
        ///     - <see cref="StatusCodes.Status400BadRequest"/> (Bad Request) with a list of errors encountered during 
        ///         the audit log deletion.
        ///     
        ///     - <see cref="StatusCodes.Status401Unauthorized"/> (Unauthorized) if the request is made by a user who is 
        ///         not authenticated or does not have the required role.
        ///   
        ///     - <see cref="StatusCodes.Status404NotFound"/> (Not Found) if the specified audit log is not found.
        /// </returns>
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = ApiDocumentation.AuditLogsApi.DeleteLog)]
        public async Task<IActionResult> DeleteLog([FromRoute][Required] string id)
        {
            var result = await _auditLogService.DeleteLog(id);

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
