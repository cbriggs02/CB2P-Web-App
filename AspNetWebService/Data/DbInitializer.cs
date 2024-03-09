using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspNetWebService.Models;

namespace AspNetWebService.Data
{
    /// <summary>
    ///     Handles database initialization and seeding.
    ///     @Author: Christian Briglio
    /// </summary>
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

            var defaultUsers = new[]
            {
                new User
                {
                    UserName = "adminTest",
                    NormalizedUserName = "adminTest",
                    FirstName = "Carson",
                    LastName = "Alexander",
                    Email = "adminTest@gmail.com",
                    NormalizedEmail = "adminTest@gmail.com",
                    BirthDate = new DateTime(1990, 1, 1),
                    PhoneNumber = "222-222-2222",
                    LockoutEnd= DateTimeOffset.UtcNow
                },
                new User
                {
                    UserName = "admin2Test",
                    NormalizedUserName = "admin2Test",
                    FirstName = "Meredith",
                    LastName = "Alonso",
                    Email = "admin2@gmail.com",
                    NormalizedEmail = "admin2@gmail.com",
                    BirthDate = new DateTime(1985, 5, 15),
                    PhoneNumber = "222-222-2222",
                    LockoutEnd= DateTimeOffset.UtcNow
                },
                new User
                {
                    UserName = "admin3Test",
                    NormalizedUserName = "admin3Test",
                    FirstName = "Arturo",
                    LastName = "Anand",
                    Email = "admin3@gmail.com",
                    NormalizedEmail = "admin3@gmail.com",
                    BirthDate = new DateTime(1988, 9, 30),
                    PhoneNumber = "222-222-2222",
                    LockoutEnd= DateTimeOffset.UtcNow
                }
            };

            foreach (var user in defaultUsers)
            {
                // Set a default password
                user.PasswordHash = passwordHasher.HashPassword(null, "P@s_s8w0rd!");
                context.Users.Add(user);
            }

            context.SaveChanges();
        }
    }
}
