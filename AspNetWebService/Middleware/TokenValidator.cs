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
                var identity = context.User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity?.Claims;

                var userClaim = claims?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

                if (userClaim == null)
                {
                    _logger.LogWarning("No valid user claim found in the token.");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
                var user = await userManager.FindByIdAsync(userClaim.Value);

                if (user == null)
                {
                    _logger.LogInformation($"User with ID {userClaim.Value} no longer exists in the system.");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized - User no longer exists");
                    return;
                }
            }
            await _next(context);
        }
    }
}
