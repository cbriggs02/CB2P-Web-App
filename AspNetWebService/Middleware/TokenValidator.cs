using IdentityServiceApi.Constants;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.UserManagement;
using System.Security.Claims;

namespace IdentityServiceApi.Middleware
{
    /// <summary>
    ///     Middleware for validating JWT tokens to ensure that tokens belonging to recently deleted users are unauthorized.
    ///     This middleware intercepts incoming requests, checks the validity of the JWT token, and verifies that the user still exists in the system.
    ///     If the user has been deleted, it marks the token as unauthorized.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class TokenValidator
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenValidator> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TokenValidator"/> class.
        /// </summary>
        /// <param name="next">
        ///     The delegate representing the next middleware in the pipeline.
        /// </param>
        /// <param name="logger">
        ///     An instance of <see cref="ILogger{TokenValidator}"/> for logging token validation data.
        /// </param>
        /// <param name="scopeFactory">
        ///     The factory for creating service scopes to resolve scoped services.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public TokenValidator(RequestDelegate next, ILogger<TokenValidator> logger, IServiceScopeFactory scopeFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }


        /// <summary>
        ///     Asynchronously validates if the JWT token in the request belongs to a user who still exists in the system.
        ///     If the user no longer exists, the request is marked as unauthorized, and an appropriate response is returned.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="HttpContext"/> for the current request, containing authentication data.
        /// </param>
        /// </param>
        /// <returns>
        ///     A task that represents the completion of the JWT token validation and request processing.
        /// </returns>
        public async Task Invoke(HttpContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

            if (context.User.Identity.IsAuthenticated)
            {
                var userId = GetUserIdFromClaims(context.User);

                if (userId == null)
                {
                    await loggerService.LogAuthorizationBreach();
                    await HandleUnauthorized(context, ErrorMessages.Authorization.MissingUserIdClaim);
                    return;
                }

                var userLookupService = scope.ServiceProvider.GetRequiredService<IUserLookupService>();
                var userLookupResult = await userLookupService.FindUserById(userId);

                if (!userLookupResult.Success)
                {
                    await loggerService.LogAuthorizationBreach();
                    await HandleUnauthorized(context, $"User with ID {userId} no longer exists in the system.");
                    return;
                }
            }
            await _next(context);
        }


        /// <summary>
        ///     Extracts the user ID from the claims contained in the <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="principal">
        ///     The <see cref="ClaimsPrincipal"/> representing the authenticated user.
        /// </param>
        /// <returns>
        /// <returns>
        ///     The user ID as a string if found in the claims; otherwise, null.
        /// </returns>
        private static string GetUserIdFromClaims(ClaimsPrincipal principal)
        {
            var identity = principal.Identity as ClaimsIdentity;
            var userClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);
            return userClaim?.Value;
        }


        /// <summary>
        ///     Asynchronously logs an unauthorized access attempt and responds with a 401 Unauthorized status code.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="HttpContext"/> for the current request.
        /// </param>
        /// <param name="reason">
        ///     The reason for the unauthorized access.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of sending the unauthorized response.
        /// </returns>
        private async Task HandleUnauthorized(HttpContext context, string reason)
        {
            _logger.LogWarning(reason);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"error\": \"{ErrorMessages.Authorization.Unauthorized}\", \"message\": \"{reason}\"}}");
        }
    }
}
