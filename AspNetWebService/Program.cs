using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspNetWebService.Mapping;
using AspNetWebService.Data;
using System.Text;
using Serilog;
using AspNetWebService.Middleware;
using AspNetWebService.Models.Entities;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Mvc;
using AspNetWebService.Interfaces.Logging;
using AspNetWebService.Services.Logging;
using AspNetWebService.Interfaces.Authentication;
using AspNetWebService.Services.Authentication;
using AspNetWebService.Interfaces.Authorization;
using AspNetWebService.Services.Authorization;
using AspNetWebService.Interfaces.UserManagement;
using AspNetWebService.Services.UserManagement;
using AspNetWebService.Interfaces.Utilities;
using AspNetWebService.Services.Utilities;
using Asp.Versioning;

namespace AspNetWebService
{
    /// <summary>
    ///     Entry point class for the ASP.NET Core application,
    ///     containing the <see cref="Main"/> method to set up and configure the application.
    /// </summary>
    /// <remarks>
    ///     Author: Christian Briglio
    /// </remarks>
    public class Program
    {
        /// <summary>
        ///     Asynchronous entry point of the application that initializes and configures the Web API.
        /// </summary>
        /// <param name="args">
        ///     Command-line arguments passed to the application.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of starting the application.
        /// </returns>
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseLazyLoadingProxies()
                    .UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDatabase"));
            });
            builder.Services.AddDbContext<HealthChecksDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("HealthChecksDatabase"));
            });

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

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add(new ProducesAttribute("application/json"));
                options.Filters.Add(new ConsumesAttribute("application/json"));
            });

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserContextService, UserContextService>();

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IUserLookupService, UserLookupService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();

            builder.Services.AddScoped<ILoginService, LoginService>();
            builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<IRoleService, RoleService>();

            builder.Services.AddScoped<IAuditLoggerService, AuditLoggerService>();
            builder.Services.AddScoped<ILoggerService, LoggerService>();
            builder.Services.AddScoped<IAuthorizationLoggerService, AuthorizationLoggerService>();
            builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();
            builder.Services.AddScoped<IPerformanceLoggerService, PerformanceLoggerService>();

            builder.Services.AddScoped<IParameterValidator, ParameterValidator>();
            builder.Services.AddScoped<IServiceResultFactory, ServiceResultFactory>();

            builder.Services.AddTransient<DbInitializer>();

            builder.Services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>("EntityFrameworkCore");
            builder.Services.AddHealthChecksUI()
                .AddSqlServerStorage(builder.Configuration.GetConnectionString("HealthChecksDatabase"));

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AspNetWebService", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token here"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                c.EnableAnnotations();
            });

            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

            var secretKey = builder.Configuration["JwtSettings:SecretKey"];
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
                app.UseMiddleware<ExceptionHandler>();
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

            app.UseMiddleware<TokenValidator>();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
