using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspNetWebService.Models.Entities;

namespace AspNetWebService.Data
{
    /// <summary>
    ///     Provides functionality for database initialization and seeding.
    ///     This class ensures that the database schema is up-to-date and 
    ///     seeds it with initial data if necessary.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class DbInitializer
    {
        private readonly ILogger<DbInitializer> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbInitializer"/> class.
        ///     This constructor injects the <see cref="ILogger{DbInitializer}"/> 
        ///     for logging.
        /// </summary>
        /// <param name="logger">
        ///     The logger used for logging messages.
        /// </param>
        public DbInitializer(ILogger<DbInitializer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Initializes the database and performs seeding if necessary.
        ///     This method is called during the application startup.
        /// </summary>
        /// <param name="app">
        ///     The <see cref="WebApplication"/> instance used to access the service provider.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public async Task InitializeDatabase(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();

                if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
                {
                    await Initialize(context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }


        /// <summary>
        ///     Applies pending migrations to the database and performs seeding
        ///     if no users are present in the database.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="ApplicationDbContext"/> instance used to interact with the database.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        private static async Task Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

            if (!context.Users.Any())
            {
                await SeedDefaultUsers(context);
            }
        }


        /// <summary>
        ///     Seeds the database with a list of default users if no users exist.
        ///     This method creates a set of default user accounts and adds them to the database.
        /// </summary>
        /// <param name="context">
        ///     The <see cref="ApplicationDbContext"/> instance used to interact with the database.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        private static async Task SeedDefaultUsers(ApplicationDbContext context)
        {
            var passwordHasher = new PasswordHasher<User>();
            var defaultUsers = new List<User>();

            for (int i = 0; i < 5000; i++)
            {
                var user = new User
                {
                    UserName = $"userTest{i}",
                    FirstName = $"FirstName{i}",
                    LastName = $"LastName{i}",
                    Email = $"userTest{i}@gmail.com",
                    PhoneNumber = "222-222-2222",
                    Country = "Canada",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    PasswordHash = passwordHasher.HashPassword(null, "P@s_s8w0rd!")
                };
                defaultUsers.Add(user);
            }

            await context.Users.AddRangeAsync(defaultUsers);
            await context.SaveChangesAsync();
        }
    }
}
