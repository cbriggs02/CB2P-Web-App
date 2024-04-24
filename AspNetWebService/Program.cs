using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspNetWebService.Helpers;
using AspNetWebService.Mapping;
using AspNetWebService.Models;
using AspNetWebService.Data;
using System.Text;
using AutoMapper;
using Serilog;
using AspNetWebService.Middleware;
using AspNetWebService.Interfaces;
using AspNetWebService.Services;

namespace AspNetWebService
{
    /// <summary>
    ///     Entry point class for the ASP.NET Core application.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class Program
    {
        /// <summary>
        ///     Main method - entry point of the application.
        ///     This method defines the structure and overall design
        ///     of the server hosting this ASP.NET Core Web API.
        /// </summary>
        /// <param name="args">
        ///     Command-line arguments.
        /// </param>
        public static void Main(string[] args)
        {

            // Set up the logger using Serilog to write to console
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);
            var configuration = new ConfigurationBuilder()

                // AddJsonFile specifies the JSON file to load configuration settings from.
                // "appsettings.json" is the name of the JSON file containing the configuration.
                // optional: false specifies that the file is required. If not found, an exception is thrown.
                // reloadOnChange: true enables automatic reloading of the configuration if the file changes.
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));
            builder.Services.AddControllers();
            builder.Services.AddScoped<IUserService, UserService>();

            // Add Swagger generation services to the service container.
            builder.Services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
            });

            // Configure and add ASP.NET Core Identity with custom User and Role classes, and password policies.
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

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

            builder.Services.AddAuthorization();

            // Manual registration of AutoMapper
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile<AutoMapperProfile>();
            });

            IMapper mapper = mapperConfig.CreateMapper();
            builder.Services.AddSingleton(mapper);

            var app = builder.Build();

            // Perform database seeding or initialization.
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();

                    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
                    {
                        DbInitializer.Initialize(context);
                    }
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

            // Catch exceptions thrown at application level and log them globally
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();

            // Log HTTPS request performance and metrics
            app.UseMiddleware<PerformanceMonitor>();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetWebService API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Run();
        }
    }
}
