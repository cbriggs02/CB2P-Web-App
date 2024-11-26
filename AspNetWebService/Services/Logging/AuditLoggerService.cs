using IdentityServiceApi.Constants;
using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DataTransferObjectModels;
using IdentityServiceApi.Models.EntityModels;
using IdentityServiceApi.Models.PaginationModels;
using IdentityServiceApi.Models.RequestModels.Logging;
using IdentityServiceApi.Models.ServiceResultModels.Common;
using IdentityServiceApi.Models.ServiceResultModels.Logging;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceApi.Services.Logging
{
    /// <summary>
    ///     Provides functionality to log actions and exceptions within the application,
    ///     including retrieving audit logs with pagination, logging unauthorized access attempts,
    ///     and logging exceptions that occur during execution.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class AuditLoggerService : IAuditLoggerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IParameterValidator _parameterValidator;
        private readonly IServiceResultFactory _serviceResultFactory;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLoggerService"/> class with the provided
        ///     application database context.
        /// </summary>
        /// <param name="context">
        ///     The database context used for accessing audit logs.
        /// </param>
        /// <param name="parameterValidator">
        ///     The paramter validator service used for defense checking service paramters.
        /// </param>
        /// <param name="mapper">
        ///     Object mapper for converting between entities and data transfer objects (DTOs).
        /// </param>
        /// <param name="serviceResultFactory">
        ///     The service used for creating the result objects being returned in operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the context is null.
        /// </exception>
        public AuditLoggerService(ApplicationDbContext context, IParameterValidator parameterValidator, IServiceResultFactory serviceResultFactory, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
            _serviceResultFactory = serviceResultFactory ?? throw new ArgumentNullException(nameof(serviceResultFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        /// <summary>
        ///     Asynchronously retrieves a paginated list of audit logs from the database.
        /// </summary>
        /// <param name="request">
        ///     Contains the pagination details for fetching audit log and audit action for optional filtering
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains
        ///     the audit logs and pagination metadata.
        /// </returns>
        public async Task<AuditLogServiceListResult> GetLogs(AuditLogListRequest request)
        {
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));

            var query = _context.AuditLogs.AsQueryable();

            if (request.Action.HasValue)
            {
                query = query.Where(x => x.Action == request.Action.Value);
            }

            var totalCount = await query.CountAsync();

            var auditLogs = await query
                .OrderBy(x => x.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var logDTOs = auditLogs.Select(log => _mapper.Map<AuditLogDTO>(log)).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            PaginationMetadata paginationMetadata = new()
            {
                TotalCount = totalCount,
                PageSize = request.PageSize,
                CurrentPage = request.Page,
                TotalPages = totalPages
            };

            return new AuditLogServiceListResult { Logs = logDTOs, PaginationMetadata = paginationMetadata };
        }


        /// <summary>
        ///     Asynchronously deletes an audit log entry by its unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the audit log entry to be deleted.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task result 
        ///     indicates whether the deletion was successful and provides any 
        ///     associated error messages if applicable.
        /// </returns>
        public async Task<ServiceResult> DeleteLog(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var log = await _context.AuditLogs.FindAsync(id);

            if (log == null)
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.AuditLog.NotFound });
            }

            _context.AuditLogs.Remove(log);
            int result = await _context.SaveChangesAsync();

            if (result == 0)
            {
                return _serviceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.AuditLog.DeletionFailed });
            }

            return _serviceResultFactory.GeneralOperationSuccess();
        }


        /// <summary>
        ///     Asynchronously logs an authorization breach event into the audit logs. This method captures details 
        ///     of unauthorized access attempts including user ID, action attempted, timestamp, and IP address.
        /// </summary>
        /// <param name="request">
        ///     The request model containing details of the unauthorized access attempt, 
        ///     such as user ID, action, and IP address.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the breach.
        /// </returns>
        public async Task LogAuthorizationBreach(AuditLogAuthorizationRequest request)
        {
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));
            _parameterValidator.ValidateNotNullOrEmpty(request.UserId, nameof(request.UserId));
            _parameterValidator.ValidateNotNullOrEmpty(request.ActionAttempted, nameof(request.ActionAttempted));
            _parameterValidator.ValidateNotNullOrEmpty(request.IpAddress, nameof(request.IpAddress));

            var log = new AuditLog
            {
                Action = AuditAction.AuthorizationBreach,
                UserId = request.UserId,
                TimeStamp = DateTime.UtcNow,
                Details = $"Unauthorized access attempt to {request.ActionAttempted}",
                IpAddress = request.IpAddress
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }


        /// <summary>
        ///     Asynchronously logs an exception to the audit logs. This method captures the 
        ///     exception details, including the message, stack trace, the user ID associated 
        ///     with the operation, and the IP address from which the request originated.
        /// </summary>
        /// <param name="request">
        ///     The request model containing the exception details, user ID, and IP address.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the exception.
        /// </returns>
        public async Task LogException(AuditLogExceptionRequest request)
        {
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));
            _parameterValidator.ValidateObjectNotNull(request.Exception, nameof(request.Exception));
            _parameterValidator.ValidateNotNullOrEmpty(request.UserId, nameof(request.UserId));
            _parameterValidator.ValidateNotNullOrEmpty(request.IpAddress, nameof(request.IpAddress));

            var log = new AuditLog
            {
                Action = AuditAction.Exception,
                UserId = request.UserId,
                TimeStamp = DateTime.UtcNow,
                Details = $"{request.Exception.Message}\n{request.Exception.StackTrace}",
                IpAddress = request.IpAddress
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }


        /// <summary>
        ///     Asynchronously logs performance metrics for requests that exceed the acceptable response time threshold.
        ///     This method creates an audit log entry for slow performance, capturing relevant details
        ///     such as the user ID, action performed, response time, and IP address.
        /// </summary>
        /// <param name="request">
        ///     The request model containing details about the slow performance like user ID, Action, Response time and Ip Address.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the performance metrics.
        /// </returns>
        public async Task LogSlowPerformance(AuditLogPerformanceRequest request)
        {
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));
            _parameterValidator.ValidateNotNullOrEmpty(request.UserId , nameof(request.UserId));
            _parameterValidator.ValidateNotNullOrEmpty(request.Action, nameof(request.Action));
            _parameterValidator.ValidateNotNullOrEmpty(request.IpAddress , nameof(request.IpAddress));

            var log = new AuditLog
            {
                Action = AuditAction.SlowPerformance,
                UserId = request.UserId,
                TimeStamp = DateTime.UtcNow,
                Details = $"Action: {request.Action}\nResponse Time: {request.ResponseTime} ms",
                IpAddress = request.IpAddress
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
