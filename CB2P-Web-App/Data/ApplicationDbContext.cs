using CB2P_Web_App.Models;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CB2P_Web_App.Data
{
    /// <summary>
    /// Represents the Entity Framework DbContext for managing application data, including IdentityServer configuration.
    /// Inherits from ApiAuthorizationDbContext to integrate IdentityServer with the ASP.NET Core Identity system.
    /// </summary>
    public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationDbContext class.
        /// </summary>
        /// <param name="options">The DbContextOptions used to create the DbContext.</param>
        /// <param name="operationalStoreOptions">The IdentityServer operational store options.</param>
        public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions)
            : base(options, operationalStoreOptions)
        {
            // Constructor implementation...
        }
    }
}
