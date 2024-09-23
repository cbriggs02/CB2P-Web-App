namespace AspNetWebService.Constants
{
    /// <summary>
    ///     Contains constants related to API documentation.
    ///     This class holds constants that are used for defining Swagger operation summaries,
    ///     descriptions, and other documentation-related strings for API endpoints.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public static class ApiDocumentation
    {
        /// <summary>
        ///     Contains constants for documenting user-related API endpoints.
        ///     These constants define summaries, descriptions, and other documentation details
        ///     specific to operations related to user management, such as creating, updating, and retrieving users.
        /// </summary>
        public static class UsersApi
        {
            public const string GetUsers = "Retrieves a paginated list of users in the system.";
            public const string GetUserById = "Retrieves a user by ID in the system.";
            public const string CreateUser = "Creates a new user in the system.";
            public const string UpdateUser = "Updates an existing user by ID in the system.";
            public const string DeleteUser = "Deletes a user by ID in the system.";
            public const string ActivateUser = "Activates a user by ID in the system.";
            public const string DeactivateUser = "Deactivates a user by ID in the system.";
        }


        /// <summary>
        ///     Contains constants for documenting password-related API endpoints.
        ///     These constants define summaries, descriptions, and other documentation details
        ///     specific to operations related to password management, such as setting, updating, and validating passwords.
        /// </summary>
        public static class PasswordApi
        {
            public const string SetPassword = "Sets a password for a user by ID in the system.";
            public const string UpdatePassword = "Updates the password for a user by ID in the system.";
        }


        /// <summary>
        ///     Contains constants for documenting login-related API endpoints.
        ///     These constants define summaries, descriptions, and other documentation details
        ///     specific to operations related to user login, including authentication and session management.
        /// </summary>
        public static class LoginApi
        {
            public const string Login = "Logs in a user to the system.";
        }


        /// <summary>
        ///     Contains constants for documenting role-related API endpoints.
        ///     These constants define summaries, descriptions, and other documentation details
        ///     specific to operations related to role management, such as assigning roles and managing permissions.
        /// </summary>
        public static class RolesApi
        {
            public const string GetRoles = "Retrieves a list of all roles in the system.";
            public const string AssignRole = "Assigns a role to a user in the system.";
            public const string RemoveRole = "Removes a role from a user in the system.";
            public const string CreateRole = "Creates a new role in the system.";
            public const string DeleteRole = "Deletes a role from the system.";
        }


        /// <summary>
        ///     Contains constants for documenting audit-log-related API endpoints.
        ///     These constants define summaries, descriptions, and other documentation details
        ///     specific to operations related to audit log management, such as retrieving list of audit logs.
        /// </summary>
        public static class AuditLogsApi
        {
            public const string GetLogs = "Retrieves a list of all audit logs in the system.";
            public const string DeleteLog = "Deletes a audit log by ID in the system.";
        }
    }
}
