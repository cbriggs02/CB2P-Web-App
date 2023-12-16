using CB2P_Web_App.Data;
using CB2P_Web_App.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CB2P_Web_App
{
    /// <summary>
    /// Entry point and configuration for this ASP.NET Core application.
    /// </summary>
    /// <remarks>
    /// This class initializes the application, sets up services, configures middleware, and defines the HTTP request processing pipeline.
    /// </remarks>
    public class Program
    {
        /// <summary>
        /// Entry point of the application.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the connection string 'DefaultConnection' is not found in the configuration.
        /// </exception>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Retrieve the connection string from configuration.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Register the ApplicationDbContext in the dependency injection container.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Add developer exception page for database-related errors.
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Configure identity-related services.

            // Configure default identity with ApplicationUser and ApplicationDbContext.
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
            {
                // Password options configuration
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.Configure<PasswordHasherOptions>(options =>
            {
                options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3; // Specify the hashing algorithm
            });

            // Configure IdentityServer for API authorization.
            builder.Services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

            // Add authentication services.
            builder.Services.AddAuthentication()
                .AddIdentityServerJwt();

            // Add MVC and Razor Pages services.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // Configures Swagger generation with Swashbuckle.
            builder.Services.AddSwaggerGen(c =>
            {
                // Enables the use of annotations in Swagger to enrich API documentation.
                c.EnableAnnotations();
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline

            // Enable migrations endpoint in development environment.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                // Use HSTS (HTTP Strict Transport Security) in production environment.
                // Note: Default HSTS value is 30 days, modify according to production requirements.
                app.UseHsts();
            }

            // Enable middleware to serve generated Swagger JSON as a Swagger endpoint.
            app.UseSwagger();

            // Enable middleware to serve Swagger UI, specifying the Swagger JSON endpoint and customizing its display.
            app.UseSwaggerUI(c =>
            {
                // Specifies the Swagger JSON endpoint and sets a title for the API documentation.
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CB2F Web App API V1");

                // Configures the route prefix for accessing the Swagger UI.
                c.RoutePrefix = string.Empty; // Route the Swagger UI to the root URL

                c.DisplayRequestDuration(); // Display request duration information
            }); ;

            // Enable static files serving, routing, authentication, and authorization.
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();

            // Configure default MVC Controller route and Razor Pages.
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");
            app.MapRazorPages();

            // Map fallback to serve index.html for unmatched routes.
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
