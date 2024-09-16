using AspNetWebService.Interfaces;
using AspNetWebService.Models;
using AspNetWebService.Models.Entities;
using AspNetWebService.Models.Result_Models.Auth_Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AspNetWebService.Services
{
    /// <summary>
    ///     Service responsible for interacting with authentication-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class AuthService : IAuthService
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        /// <summary>
        ///     Constructor for the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="signInManager">
        ///     The sign-in manager used for user authentication.
        /// </param>
        /// <param name="userManager">
        ///     The user manager used for managing user-related operations.
        /// </param>
        /// <param name="configuration">
        ///     The configuration used for accessing app settings, including JWT settings.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any of the parameters are null.
        /// </exception>
        public AuthService(SignInManager<User> signInManager, UserManager<User> userManager, IConfiguration configuration)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }


        /// <summary>
        ///     Logs in a user in system using sign in manager based on user credentials.
        /// </summary>
        /// <param name="credentials">
        ///     Required information used to authenticate a user inside the system upon logging in.
        /// </param>
        /// <returns>
        ///     Returns a AuthResult Object indicating the login status.
        ///     - If successful, returns a AuthResult with success set to true.
        ///     - If the provided username could not be located in the system returns a error message.
        ///     - If the provided password does not match the located user in the system returns a error message.
        ///     - If an error occurs during login, returns AuthResult with error message.
        /// </returns>
        public async Task<AuthResult> Login(LoginRequest credentials)
        {
            var user = await _userManager.FindByNameAsync(credentials.UserName);

            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "User not found." }
                };
            }

            if (user.AccountStatus == 0)
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "This user account has not been activated inside the system yet." }
                };
            }

            var result = await _signInManager.PasswordSignInAsync(user, credentials.Password, false, true);

            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user);

                return new AuthResult
                {
                    Success = true,
                    Token = token,
                };
            }
            else
            {
                return new AuthResult
                {
                    Success = false,
                    Errors = new List<string> { "Invalid password." }
                };
            }
        }


        /// <summary>
        ///     Generates a JWT token for the specified user based on configured settings.
        /// </summary>
        /// <param name="user">
        ///     The user for whom the token is generated.
        /// </param>
        /// <returns>
        ///     A string representing the generated JWT token.
        /// </returns>
        private string GenerateJwtToken(User user)
        {
            var validIssuer = _configuration["JwtSettings:ValidIssuer"];
            var validAudience = _configuration["JwtSettings:ValidAudience"];

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName),
            };

            var tokenOptions = new JwtSecurityToken(
                issuer: validIssuer,
                audience: validAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: signingCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return tokenString;
        }
    }
}
