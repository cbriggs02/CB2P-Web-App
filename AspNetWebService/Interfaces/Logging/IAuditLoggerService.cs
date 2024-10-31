using AspNetWebService.Models.RequestModels.AuditLogRequests;
using AspNetWebService.Models.ServiceResultModels;
using AspNetWebService.Models.ServiceResultModels.AuditLogServiceResults;

namespace AspNetWebService.Interfaces.Logging
{
    /// <summary>
    ///     Defines the contract for audit log services that manage logging of various actions 
    ///     within the application, such as logging authorization breaches and exceptions.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public interface IAuditLoggerService
    {
        /// <summary>
        ///     Asynchronously retrieves a paginated list of audit logs based on the provided request parameters.
        /// </summary>
        /// <param name="request">
        ///     The request parameters including pagination details such as page number, size, and filters.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of retrieving logs. 
        ///     The result contains a list of audit logs along with pagination metadata.
        /// </returns>
        Task<AuditLogServiceListResult> GetLogs(AuditLogListRequest request);


        /// <summary>
        ///     Asynchronously deletes a audit log in the system by ID.
        /// </summary>
        /// <param name="id">
        ///     The ID used to locate the audit log to be deleted in the system.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation that returns a <see cref= ServiceResult"/> object.
        /// </returns>
        Task<ServiceResult> DeleteLog(string id);


        /// <summary>
        ///     Asynchronously logs an authorization breach event. This method captures details 
        ///     such as the user ID, the action attempted, the timestamp, and the IP address
        ///     of an unauthorized access attempt for audit logging purposes.
        /// </summary>
        /// <param name="request">
        ///     The request model containing details about the unauthorized access attempt,
        ///     including user ID, action attempted, and IP address.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the authorization breach.
        /// </returns>
        Task LogAuthorizationBreach(AuditLogAuthorizationRequest request);


        /// <summary>
        ///     Asynchronously logs an exception that occurs during system operations. This method 
        ///     captures the exception details, the user ID associated with the request, and the 
        ///     IP address from which the request originated.
        /// </summary>
        /// <param name="request">
        ///     The request model containing details about the exception, including the 
        ///     exception object, user ID, and IP address.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of logging the exception.
        /// </returns>
        Task LogException(AuditLogExceptionRequest request);


        /// <summary>
        ///     Asynchronously logs performance metrics for slow requests. This method captures the details of a request 
        ///     that exceeded the acceptable performance threshold.
        /// </summary>
        /// <param name="request">
        ///     An instance of <see cref="AuditLogPerformanceRequest"/> containing details about the performance issue,
        ///     including user ID, action, response time, and IP address.
        /// </param>
        /// <returns>
        ///     A task repres
        /// </returns>
        Task LogSlowPerformance(AuditLogPerformanceRequest request);
    }
}
