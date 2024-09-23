﻿using AspNetWebService.Models.DataTransferObjectModels;
using AspNetWebService.Models.PaginationModels;

namespace AspNetWebService.Models.ServiceResultModels.UserServiceResults
{
    /// <summary>
    ///     Represents the result of a user-related operation 
    ///     performed by the user service, including a list of users 
    ///     and pagination metadata.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class UserServiceListResult
    {
        /// <summary>
        ///     Contains users retrieved from the operation.
        ///     This list may be empty if no users match the request criteria.
        /// </summary>  
        public IEnumerable<UserDTO> Users { get; set; } = new List<UserDTO>();

        /// <summary>
        ///     Gets or sets the pagination metadata associated with the user retrieval operation.
        ///     This metadata includes details such as total user count, current page, and page size. 
        /// </summary>
        public PaginationMetadata PaginationMetadata { get; set; }
    }
}