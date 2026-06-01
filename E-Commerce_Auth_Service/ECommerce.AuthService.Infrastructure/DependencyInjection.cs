using ECommerce.AuthService.Application.Interfaces;
using ECommerce.AuthService.Domain.Entities;
using ECommerce.AuthService.Infrastructure.Persistence;
using ECommerce.AuthService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Wolverine.EntityFrameworkCore;
namespace ECommerce.AuthService.Infrastructure;

/// <summary>
/// Extension methods for registering all Infrastructure layer dependencies.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers persistence, identity, authentication, messaging, and infrastructure services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddPersistence(configuration)
            .AddIdentityConfiguration()
            .AddJwtAuthentication(configuration)
            .AddInfrastructureServices();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextWithWolverineIntegration<AuthDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Constr")));

        return services;
    }

    private static IServiceCollection AddIdentityConfiguration(
        this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 10;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequiredUniqueChars = 3;

            // User settings
            options.User.RequireUniqueEmail = true;

            // Sign-in settings
            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedPhoneNumber = false;
            options.SignIn.RequireConfirmedAccount = true;

            // Lockout settings – brute force protection
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.AllowedForNewUsers = true;
        });

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["Jwt:Key"]
                        ?? Environment.GetEnvironmentVariable("JWT_KEY");

        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT Key is not configured.");

        var issuer = configuration["Jwt:Issuer"]
                     ?? Environment.GetEnvironmentVariable("JWT_ISSUER");

        var audience = configuration["Jwt:Audience"]
                       ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey))
            };
        });

        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
