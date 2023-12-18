using AspNetWebService.Data;
using AspNetWebService.Models;
using AspNetWebService.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;

namespace AspNetWebService
{
    /// <summary>
    /// Entry point class for the ASP.NET Core application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main method - entry point of the application.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static void Main(string[] args)
        {

            // Set up the logger using Serilog
            Log.Logger = new LoggerConfiguration()
                // Write log events to the console
                .WriteTo.Console()
                // Create the logger instance
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            // Creates a ConfigurationBuilder to build configuration settings for the application.
            var configuration = new ConfigurationBuilder()

                // AddJsonFile specifies the JSON file to load configuration settings from.
                // "appsettings.json" is the name of the JSON file containing the configuration.
                // optional: false specifies that the file is required. If not found, an exception is thrown.
                // reloadOnChange: true enables automatic reloading of the configuration if the file changes.
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();


            // Retrieves the connection string named "DefaultConnection" from the loaded configuration.
            var connection = builder.Configuration.GetConnectionString("DefaultConnection");

            // Adds the ApplicationDbContext to the services with the specified SQL Server connection string.
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));

            // Add Controllers to the service container.
            builder.Services.AddControllers();

            // Add Swagger generation services to the service container.
            builder.Services.AddSwaggerGen(c =>
            {
                // Enable annotations (XML comments, attributes) to be reflected in the generated Swagger/OpenAPI document.
                c.EnableAnnotations();
            });

            // Configure and add ASP.NET Core Identity with custom User and Role classes, and password policies.
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                // Password options configuration
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>(); // Use EF Core for storing Identity data

            // Configure options for password hashing compatibility mode.
            builder.Services.Configure<PasswordHasherOptions>(options =>
            {
                options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            });

            // Use the SecretKeyGenerator to generate a secret key dynamically
            var secretKey = SecretKeyGenerator.GenerateRandomSecretKey(32);

            // Read JwtSettings from appsettings.json
            var validIssuer = configuration["JwtSettings:ValidIssuer"];
            var validAudience = configuration["JwtSettings:ValidAudience"];

            // Configure JWT authentication services.
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true, // Validate the issuer of the token
                   ValidateAudience = true, // Validate the audience of the token
                   ValidateLifetime = true, // Validate the lifetime of the token
                   ValidateIssuerSigningKey = true, // Validate the security key used to sign the token

                   // Use dynamically generated secret key
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                   ValidIssuer = validIssuer,
                   ValidAudience = validAudience
               };
           });

            // Configure authorization services
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Perform database seeding or initialization.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    DbInitializer.Initialize(context); // Seed the database if necessary
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable Swagger and Swagger UI for API documentation.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Students API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            // Enable authentication and authorization middleware.
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // Map controllers for handling requests
            });

            app.Run(); // Start the application
        }
    }
}
