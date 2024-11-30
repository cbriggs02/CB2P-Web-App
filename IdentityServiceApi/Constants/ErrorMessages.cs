namespace IdentityServiceApi.Constants
{
    /// <summary>
    ///     Contains constants for error messages used throughout the application.
    ///     These messages are used for providing consistent and descriptive error responses.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public static class ErrorMessages
    {
        /// <summary>
        ///     Error messages related to general exceptions and global errors.
        /// </summary>
        public static class General
        {
            public const string GlobalExceptionMessage = "An unexpected error occurred. Please try again later.";
            public const string NotFound = "The requested resource was not found.";
        }

        /// <summary>
        ///     Error messages related to database operations.
        /// </summary>
        public static class Database
        {
            public const string UpdateFailed = "An error occurred while updating the database.";
            public const string InitializationFailed = "An unexpected error occurred during database initialization.";
        }

        /// <summary>
        ///     Error messages related to authorization and authentication.
        /// </summary>
        public static class Authorization
        {
            public const string Forbidden = "Access is forbidden.";
            public const string Unauthorized = "User is not authorized to access this resource.";
            public const string NotAuthenticated = "User is not authenticated.";
            public const string MissingUserIdClaim = "User ID claim is missing in the authentication token.";
        }

        /// <summary>
        ///     Error messages related to user operations.
        /// </summary>
        public static class User
        {
            public const string NotFound = "User not found.";
            public const string AlreadyActivated = "User account is already activated.";
            public const string NotActivated = "User account is not activated.";
        }

        /// <summary>
        ///     Error messages related to role management.
        /// </summary>
        public static class Role
        {
            public const string InvalidRole = "The specified role is invalid.";
            public const string NotFound = "Role not found.";
            public const string AlreadyExist = "Role already exists.";
            public const string InactiveUser = "Cannot assign role to an inactive user account.";
            public const string MissingRole = "The specified role is not assigned to the user.";
            public const string HasRole = "The user is already assigned this role.";
        }

        /// <summary>
        ///     Error messages related to password operations.
        /// </summary>
        public static class Password
        {
            public const string Mismatch = "Passwords do not match.";
            public const string AlreadySet = "Password has already been set.";
            public const string InvalidCredentials = "Credentials are invalid.";
            public const string CannotReuse = "Cannot reuse the previous password. Please choose a new one.";
        }

        /// <summary>
        ///     Error messages related to audit log operations.
        /// </summary>
        public static class AuditLog
        {
            public const string NotFound = "Audit log not found.";
            public const string DeletionFailed = "Failed to delete the audit log.";
            public const string InvalidDate = "The timestamp in the audit log must match the current UTC time.";
            public const string InvalidAction = "The provided audit action is invalid. Please ensure it is a valid value from the AuditAction enum.";

            /// <summary>
            ///     Error messages specific to performance log operations.
            /// </summary>
            public static class PerformanceLog
            {
                public const string InvalidResponseTime = "Invalid response time. Please ensure the response time is greater than zero.";
            }
        }
    }
}
