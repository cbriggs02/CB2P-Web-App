using AspNetWebService.Constants;
using AspNetWebService.Interfaces.Authentication;
using AspNetWebService.Models.Entities;
using AspNetWebService.Models.RequestModels.LoginRequests;
using AspNetWebService.Models.ServiceResultModels.LoginServiceResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AspNetWebService.Services.Authentication
{
    /// <summary>
    ///     Service responsible for interacting with login-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class LoginService : ILoginService
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoginService"/> class.
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
        public LoginService(SignInManager<User> signInManager, UserManager<User> userManager, IConfiguration configuration)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        /// <summary>
        ///     Asynchronously logs in a user in the system using the sign-in manager based on provided credentials.
        /// </summary>
        /// <param name="credentials">
        ///     Required information used to authenticate the user during login.
        /// </param>
        /// <returns>
        ///     Returns a <see cref="LoginServiceResult"/> indicating the login status.   
        ///     - If successful, returns a <see cref="LoginServiceResult"/> with Success set to true.    
        ///     - If the provided username could not be located, returns an error message.
        ///     - If the provided password does not match the located user, returns an error message.  
        ///     - If an error occurs during login, returns <see cref="LoginServiceResult"/> with an error message.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when if credentials are null.
        /// </exception>
        public async Task<LoginServiceResult> Login(LoginRequest credentials)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            var user = await _userManager.FindByNameAsync(credentials.UserName);

            if (user == null)
            {
                return GenerateErrorResult(ErrorMessages.User.NotFound);
            }

            if (user.AccountStatus == 0)
            {
                return GenerateErrorResult(ErrorMessages.User.NotActivated);
            }
            
            var result = await _signInManager.PasswordSignInAsync(user, credentials.Password, false, true);

            if (result.Succeeded)
            {
                var token = await GenerateJwtToken(user);
                return GenerateSuccsesResult(token);
            }
            else
            {
                return GenerateErrorResult(ErrorMessages.Password.InvalidCredentials);
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
        private async Task<string> GenerateJwtToken(User user)
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

            var roles = await _userManager.GetRolesAsync(user);

            if (roles != null && roles.Any())
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
           
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


        /// <summary>
        ///     Generates a login service result representing an error, with success set to false.
        /// </summary>
        /// <param name="errorMessage">
        ///     The error message to include in the result.
        /// </param>
        /// <returns>
        ///     A login service result indicating failure, with a list of error messages.
        /// </returns>
        private static LoginServiceResult GenerateErrorResult(string errorMessage)
        {
            return new LoginServiceResult
            {
                Success = false,
                Errors = new List<string> { errorMessage }
            };
        }


        /// <summary>
        ///     Generates a login service result representing a successful login.
        /// </summary>
        /// <param name="token">
        ///     The JWT token issued for the authenticated user.
        /// </param>
        /// <returns>
        ///     A login service result indicating success, with the generated token.
        /// </returns>
        private static LoginServiceResult GenerateSuccsesResult(string token)
        {
            return new LoginServiceResult
            {
                Success = true,
                Token = token,
            };
        }
    }
}
