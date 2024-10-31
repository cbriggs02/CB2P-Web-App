using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.ServiceResultModels;
using AspNetWebService.Models.ServiceResultModels.LoginServiceResults;
using AspNetWebService.Models.ServiceResultModels.UserServiceResults;

namespace AspNetWebService.Interfaces.Utilities
{
    /// <summary>
    ///     Provides methods for creating uniform service result objects 
    ///     for various operations within the application. This factory 
    ///     centralizes the creation logic for service results, ensuring 
    ///     consistency and reducing duplication in the codebase.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public interface IServiceResultFactory
    {
        /// <summary>
        ///     Creates a successful service result for general operations.
        /// </summary>
        ServiceResult GeneralOperationSuccess();


        /// <summary>
        ///     Creates a successful login service result with a token.
        /// </summary>
        LoginServiceResult LoginOperationSuccess(string token);

    
        /// <summary>
        ///     Creates a successful user operation result with a user DTO.
        /// </summary>
        UserServiceResult UserOperationSuccess(UserDTO user);


        /// <summary>
        ///     Creates a failed service result with specified errors for general operations.
        /// </summary>
        ServiceResult GeneralOperationFailure(string[] errors);


        /// <summary>
        ///     Creates a failed service result with specified errors for login operations.
        /// </summary>
        LoginServiceResult LoginOperationFailure(string[] errors);


        /// <summary>
        ///     Creates a failed service result with specified errors for user operations.
        /// </summary>
        UserServiceResult UserOperationFailure(string[] errors);
    }
}
