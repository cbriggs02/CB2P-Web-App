using Microsoft.EntityFrameworkCore;
using AspNetWebService.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AspNetWebService.Data
{
    /// <summary>
    ///     Represents the database context for the application.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">
        ///     The options to be used by the database context.
        /// </param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        /// <summary>
        ///     Configures relationships and keys for entities in the database.
        /// </summary>
        /// <param name="modelBuilder">
        ///     The model builder used to define the database schema.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .ToTable("Users")
                .HasKey(x => x.Id);
        }
    }
}
