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
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                await context.Database.MigrateAsync();

                if (app.Environment.IsDevelopment())
                {
                    await InitializeUsers(userManager, roleManager);
                }

                await InitializeRoles(roleManager);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "An error occurred while updating the database.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during database initialization.");
            }
        }


        /// <summary>
        ///     Initializes users by seeding default users and the admin user if no users exist.
        /// </summary>
        /// <param name="userManager">
        ///     The <see cref="UserManager{TUser}"/> used to manage user creation.
        /// </param>
        /// <param name="roleManager">
        ///     The <see cref="RoleManager{IdentityRole}"/> used to manage roles.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        private static async Task InitializeUsers(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            if (!userManager.Users.Any())
            {
                await SeedDefaultUsers(userManager);
                await SeedAdmin(userManager, roleManager);
            }
        }


        /// <summary>
        ///     Initializes roles by seeding them if they do not already exist.
        /// </summary>
        /// <param name="roleManager">
        ///     The <see cref="RoleManager{IdentityRole}"/> used to manage roles.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        private static async Task InitializeRoles(RoleManager<IdentityRole> roleManager)
        {
            await SeedRoles(roleManager);
        }


        /// <summary>
        ///     Seeds the database with a list of default users if no users exist.
        /// </summary>
        /// <param name="userManager">
        ///     The <see cref="UserManager{TUser}"/> used to manage user creation.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        private static async Task SeedDefaultUsers(UserManager<User> userManager)
        {
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
                    UpdatedAt = DateTime.UtcNow
                };

                await userManager.CreateAsync(user, "P@s_s8w0rd!");
            }
        }


        /// <summary>
        ///     Seeds an admin user with a specified email and assigns the "Admin" role.
        /// </summary>
        /// <param name="userManager">
        ///     The <see cref="UserManager{TUser}"/> used to manage user creation and role assignment.
        /// </param>
        /// <param name="roleManager">
        ///     The <see cref="RoleManager{IdentityRole}"/> used to manage roles.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        private static async Task SeedAdmin(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            const string adminEmail = "admin@admin.com";
            const string adminPassword = "AdminPassword123!";
            const string adminRole = "Admin";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    FirstName = "Robert",
                    LastName = "Plankton",
                    Email = adminEmail,
                    PhoneNumber = "222-222-2222",
                    Country = "Canada",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    if (!await roleManager.RoleExistsAsync(adminRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(adminRole));
                    }
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
            }
        }


        /// <summary>
        ///     Seeds the default roles in the database if they do not already exist.
        /// </summary>
        /// <param name="roleManager">
        ///     The <see cref="RoleManager{IdentityRole}"/> used to manage roles.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
