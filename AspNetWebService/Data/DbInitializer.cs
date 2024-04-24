using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspNetWebService.Models;

namespace AspNetWebService.Data
{
    /// <summary>
    ///     Handles database initialization and seeding.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public static class DbInitializer
    {
        /// <summary>
        ///     Ensures the database is created and seeds initial data if necessary.
        /// </summary>
        /// <param name="context">
        ///     The application's database context.
        /// </param>
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

            if (!context.Users.Any())
            {
                SeedDefaultUsers(context);
            }
        }

        /// <summary>
        ///     Seeds the database with default users if no users exist.
        /// </summary>
        /// <param name="context">
        ///     The application's database context.
        /// </param>
        private static void SeedDefaultUsers(ApplicationDbContext context)
        {
            var passwordHasher = new PasswordHasher<User>();
            var defaultUsers = new List<User>();

            for (int i = 0; i < 1000; i++)
            {
                var user = new User
                {
                    UserName = $"userTest{i}",
                    NormalizedUserName = $"userTest{i}",
                    FirstName = $"FirstName{i}",
                    LastName = $"LastName{i}",
                    Email = $"userTest{i}@gmail.com",
                    NormalizedEmail = $"userTest{i}@gmail.com",
                    BirthDate = new DateTime(1990, 1, 1),
                    PhoneNumber = "222-222-2222",
                    Country = "Canada",
                    LockoutEnd = DateTimeOffset.UtcNow
                };

                // Set a default password
                user.PasswordHash = passwordHasher.HashPassword(null, "P@s_s8w0rd!");
                defaultUsers.Add(user);
            }

            context.Users.AddRange(defaultUsers);
            context.SaveChanges();
        }
    }
}
