using AspNetWebService.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AspNetWebService.Middleware
{
    /// <summary>
    ///     Middleware for validating JWT Tokens to ensure that tokens belonging to recently deleted users are unauthorized
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class TokenValidator
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidator> _logger;
        private const string UnauthorizedMessage = "Unauthorized";

        /// <summary>
        ///     Initializes a new instance of the <see cref="TokenValidator"/> class.
        /// </summary>
        /// <param name="next">
        ///     The delegate representing the next middleware in the pipeline.
        /// </param>
        /// <param name="logger">
        ///     The logger instance for logging token validation data.
        /// </param>
        public TokenValidator(RequestDelegate next, ILogger<TokenValidator> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        ///     Middleware method that validates if the JWT token belongs to a user that still exists in the system.
        /// </summary>
        /// <param name="context">
        ///     The HttpContext for the current request, containing user authentication data.
        /// </param>
        /// <param name="serviceProvider">
        ///     The service provider used to resolve scoped services like UserManager.
        /// </param>
        /// <returns>
        ///     A task that represents the completion of the token validation and subsequent request handling.
        /// </returns>
        public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var userId = GetUserIdFromClaims(context.User);

                if (userId == null)
                {
                    await HandleUnauthorized(context, "User ID claim is missing in the token.");
                    return;
                }

                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
                var user = await userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    await HandleUnauthorized(context, $"User with ID {userId} no longer exists in the system.");
                    return;
                }
            }
            await _next(context);
        }


        /// <summary>
        ///     Retrieves the user ID from claims.
        /// </summary>
        /// <param name="principal">
        ///     The claims principal.
        /// </param>
        /// <returns>
        ///     The user ID if found; otherwise, null.
        /// </returns>
        private static string GetUserIdFromClaims(ClaimsPrincipal principal)
        {
            var identity = principal.Identity as ClaimsIdentity;
            var userClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);
            return userClaim?.Value;
        }


        /// <summary>
        ///     Logs an unauthorized access attempt and writes the corresponding response.
        /// </summary>
        /// <param name="context">
        ///     The current HttpContext.
        /// </param>
        /// <param name="reason">
        ///     The reason for the unauthorized access.
        /// </param>
        private async Task HandleUnauthorized(HttpContext context, string reason)
        {
            _logger.LogWarning(reason);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"error\": \"{UnauthorizedMessage}\", \"message\": \"{reason}\"}}");
        }
    }
}
