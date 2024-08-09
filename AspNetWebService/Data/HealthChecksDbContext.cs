using Microsoft.EntityFrameworkCore;

namespace AspNetWebService.Data
{
    /// <summary>
    ///     Represents the DbContext for HealthChecks UI.
    ///     This context is used to interact with the database for storing health check data.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class HealthChecksDbContext : DbContext
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HealthChecksDbContext"/> class.
        /// </summary>
        /// <param name="options">
        ///     The options to configure the <see cref="HealthChecksDbContext"/>.
        /// </param>
        public HealthChecksDbContext(DbContextOptions<HealthChecksDbContext> options) : base(options)
        {

        }
    }
}
