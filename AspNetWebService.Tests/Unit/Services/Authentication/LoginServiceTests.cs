using AspNetWebService.Constants;
using AspNetWebService.Models.Entities;
using AspNetWebService.Models.RequestModels.LoginRequests;
using AspNetWebService.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AspNetWebService.Tests.UnitTests.Services.Authentication
{
    /// <summary>
    ///     Unit tests for the <see cref="LoginService"/> class.
    ///     This class contains test cases for various login scenarios, verifying the 
    ///     behavior of the login functionality.
    /// </summary>
    /// <remarks>
    ///     Author: Christian Briglio
    /// </remarks>
    [Trait("Category", "UnitTest")]
    public class LoginServiceTests
    {
        private const string NonExistentUserName = "nonexistent";
        private const string TestPassword = "testpassword";
        private const string CorrectPassword = "correctpassword";
        private const string WrongPassword = "wrongpassword";
        private const string ValidIssuer = "issuer";
        private const string ValidAudience = "audience";
        private const string SecretKey = "superSecretKey123!";

        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<UserManager<User>>> _userManagerLoggerMock;
        private readonly Mock<ILogger<SignInManager<User>>> _signInManagerLoggerMock;
        private readonly Mock<IUserStore<User>> _userStoreMock;
        private readonly Mock<IOptions<IdentityOptions>> _optionsMock;
        private readonly Mock<IPasswordHasher<User>> _userHasherMock;
        private readonly Mock<IUserValidator<User>> _userValidatorMock;
        private readonly Mock<IPasswordValidator<User>> _passwordValidatorsMock;
        private readonly Mock<ILookupNormalizer> _keyNormalizerMock;
        private readonly Mock<IdentityErrorDescriber> _errorsMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IUserClaimsPrincipalFactory<User>> _claimsFactoryMock;
        private readonly Mock<IAuthenticationSchemeProvider> _schemesMock;
        private readonly Mock<IUserConfirmation<User>> _confirmationMock;
        private readonly LoginService _loginService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoginServiceTests"/> class.
        ///     This constructor sets up the mocked dependencies and creates an instance 
        ///     of the <see cref="LoginService"/> for testing.
        /// </summary>
        public LoginServiceTests()
        {
            _userStoreMock = new Mock<IUserStore<User>>();
            _optionsMock = new Mock<IOptions<IdentityOptions>>();
            _userHasherMock = new Mock<IPasswordHasher<User>>();
            _userValidatorMock = new Mock<IUserValidator<User>>();
            _passwordValidatorsMock = new Mock<IPasswordValidator<User>>();
            _keyNormalizerMock = new Mock<ILookupNormalizer>();
            _errorsMock = new Mock<IdentityErrorDescriber>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _userManagerLoggerMock = new Mock<ILogger<UserManager<User>>>();
            _signInManagerLoggerMock = new Mock<ILogger<SignInManager<User>>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            _schemesMock = new Mock<IAuthenticationSchemeProvider>();
            _confirmationMock = new Mock<IUserConfirmation<User>>();

            _optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());

            _userManagerMock = new Mock<UserManager<User>>(
                _userStoreMock.Object,
                _optionsMock.Object,
                _userHasherMock.Object,
                new[] { _userValidatorMock.Object },
                new[] { _passwordValidatorsMock.Object },
                _keyNormalizerMock.Object,
                _errorsMock.Object,
                _serviceProviderMock.Object,
                _userManagerLoggerMock.Object
            );

            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                _httpContextAccessorMock.Object,
                _claimsFactoryMock.Object,
                _optionsMock.Object,
                _signInManagerLoggerMock.Object,
                _schemesMock.Object,
                _confirmationMock.Object
            );

            _configurationMock = new Mock<IConfiguration>();

            // Create the LoginService instance for testing
            _loginService = new LoginService(_signInManagerMock.Object, _userManagerMock.Object, _configurationMock.Object);
        }


        /// <summary>
        ///     Tests the login functionality when the user is not found.
        ///     Expects a result indicating the user was not found.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task Login_UserNotFound_ReturnsNotFoundResult()
        {
            var request = new LoginRequest { UserName = NonExistentUserName, Password = TestPassword };

            _userManagerMock
                .Setup(x => x.FindByNameAsync(NonExistentUserName))
                .ReturnsAsync((User)null);

            var result = await _loginService.Login(request);

            Assert.False(result.Success);
            Assert.Contains(ErrorMessages.User.NotFound, result.Errors);
        }


        /// <summary>
        ///     Tests the login functionality when the user is not activated.
        ///     Expects a result indicating the user is not activated.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task Login_UserNotActivated_ReturnsNotActivatedResult()
        {
            var inactiveUser = CreateMockUser(false);

            _userManagerMock
                .Setup(x => x.FindByNameAsync(inactiveUser.UserName))
                .ReturnsAsync(inactiveUser);

            var request = CreateRequestObject(TestPassword, inactiveUser);

            var result = await _loginService.Login(request);

            Assert.False(result.Success);
            Assert.Contains(ErrorMessages.User.NotActivated, result.Errors);
        }


        /// <summary>
        ///     Tests the login functionality with invalid credentials.
        ///     Expects a result indicating invalid credentials were provided.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task Login_InvalidCredentials_ReturnsInvalidCredentialsResult()
        {
            var user = CreateMockUser(true);

            _userManagerMock
                .Setup(x => x.FindByNameAsync(user.UserName))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(j => j.PasswordSignInAsync(user, WrongPassword, false, true))
                .ReturnsAsync(SignInResult.Failed);

            var request = CreateRequestObject(WrongPassword, user);

            var result = await _loginService.Login(request);

            Assert.False(result.Success);
            Assert.Contains(ErrorMessages.Password.InvalidCredentials, result.Errors);
        }


        /// <summary>
        ///     Tests the login functionality for a successful login.
        ///     Expects a result indicating success and a valid JWT token.
        /// </summary>
        /// <returns>
        ///     A task representing the asynchronous operation.
        /// </returns>
        [Fact]
        public async Task Login_SuccessfulLogin_ReturnsToken()
        {
            var mockUser = CreateMockUser(true);

            _userManagerMock
                .Setup(x => x.FindByNameAsync(mockUser.UserName))
                .ReturnsAsync(mockUser);

            _signInManagerMock
                .Setup(j => j.PasswordSignInAsync(mockUser, CorrectPassword, false, true))
                .ReturnsAsync(SignInResult.Success);

            SetupConfiguration(ValidIssuer, ValidAudience, SecretKey);

            var request = CreateRequestObject(CorrectPassword, mockUser);

            var result = await _loginService.Login(request);

            Assert.True(result.Success);
            Assert.NotNull(result.Token);
        }


        /// <summary>
        ///     Sets up the configuration mock with the given JWT settings.
        /// </summary>
        /// <param name="issuer">
        ///     The valid issuer value.
        /// </param>
        /// <param name="audience">
        ///     The valid audience value.
        /// </param>
        /// <param name="secretKey">
        ///     The secret key value.
        /// </param>
        private void SetupConfiguration(string issuer, string audience, string secretKey)
        {
            _configurationMock
                .Setup(c => c["JwtSettings:ValidIssuer"])
                .Returns(issuer);
            _configurationMock
                .Setup(c => c["JwtSettings:ValidAudience"])
                .Returns(audience);
            _configurationMock
                .Setup(c => c["JwtSettings:SecretKey"])
                .Returns(secretKey);
        }


        /// <summary>
        ///     Creates a new <see cref="LoginRequest"/> object with the specified password and user information.
        /// </summary>
        /// <param name="password">
        ///     The password to use for the login request.
        /// </param>
        /// <param name="user">
        ///     The user object containing the username to be included in the login request.
        /// </param>
        /// <returns>
        ///     A <see cref="LoginRequest"/> object initialized with the provided username and password.
        /// </returns>
        private static LoginRequest CreateRequestObject(string password, User user)
        {
            return new LoginRequest { UserName = user.UserName, Password = password };
        }


        /// <summary>
        ///     Creates a mock <see cref="User"/> object with the specified account status.
        /// </summary>
        /// <param name="accountStatus">
        ///     The account status to set for the mock user. This could indicate whether the user is active or inactive.
        /// </param>
        /// <returns>
        ///     A <see cref="User"/> object initialized with the specified account status and a predefined username.
        /// </returns>
        private static User CreateMockUser(bool accountStatus)
        {
            return new User { UserName = "testuser", AccountStatus = accountStatus ? 1 : 0};
        }
    }
}
