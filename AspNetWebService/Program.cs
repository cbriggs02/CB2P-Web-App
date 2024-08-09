using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspNetWebService.Helpers;
using AspNetWebService.Mapping;
using AspNetWebService.Data;
using System.Text;
using Serilog;
using AspNetWebService.Middleware;
using AspNetWebService.Interfaces;
using AspNetWebService.Services;
using AspNetWebService.Models.Entities;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

namespace AspNetWebService
{
    /// <summary>
    ///     Entry point class for the ASP.NET Core application.
    ///     This class contains the main method that serves as the entry point for the application.
    ///     It sets up and configures the ASP.NET Core Web API application.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    /// </remarks>
    public class Program
    {
        /// <summary>
        ///     Main method - entry point of the application.
        ///     This method initializes and configures the ASP.NET Core Web API application.
        ///     It builds the application host, configures services, and sets up the HTTP request pipeline.
        ///     The method is asynchronous and supports asynchronous operations during startup.
        /// </summary>
        /// <param name="args">
        ///     Command-line arguments passed to the application.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of starting the application.
        /// </returns>
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDatabase")));
            builder.Services.AddDbContext<HealthChecksDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("HealthChecksDatabase")));

            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.Configure<PasswordHasherOptions>(options =>
            {
                options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            });

            builder.Services.AddControllers();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();
            builder.Services.AddTransient<DbInitializer>();

            builder.Services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>("EntityFrameworkCore");
            builder.Services.AddHealthChecksUI()
                .AddSqlServerStorage(builder.Configuration.GetConnectionString("HealthChecksDatabase"));

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AspNetWebService", Version = "v1" });
                c.EnableAnnotations();
            });

            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

            var secretKey = SecretKeyGenerator.GenerateRandomSecretKey(32);
            var validIssuer = builder.Configuration["JwtSettings:ValidIssuer"];
            var validAudience = builder.Configuration["JwtSettings:ValidAudience"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,

                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                   ValidIssuer = validIssuer,
                   ValidAudience = validAudience
               };
           });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
                await dbInitializer.InitializeDatabase(app);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetWebService API V1");
                    c.RoutePrefix = string.Empty;
                });
            }
            else
            {
                app.UseMiddleware<ExceptionMiddleware>();
            }

            app.UseMiddleware<PerformanceMonitor>();

            app.UseHttpsRedirection();

            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecks("/health/database", new HealthCheckOptions()
            {
                Predicate = registration => registration.Name == "EntityFrameworkCore",
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(config => config.UIPath = "/health-ui");

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
