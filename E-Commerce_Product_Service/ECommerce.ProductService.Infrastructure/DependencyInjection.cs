using ECommerce.ProductService.Infrastructure.Idempotency;
using ECommerce.ProductService.Application.Common.Interfaces;
using ECommerce.ProductService.Infrastructure.Persistence;
using ECommerce.ProductService.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Wolverine.EntityFrameworkCore;

namespace ECommerce.ProductService.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructureServices(
			this IServiceCollection services,
			IConfiguration configuration)
		{
			// ── Database ─────────────────────────────────────────────────────────
			var connectionString = configuration.GetConnectionString(
				"Constr");

			var optionsAction = (Action<DbContextOptionsBuilder>)(options =>
				options.UseSqlServer(connectionString));

			if (configuration.GetValue("Wolverine:SqlPersistenceEnabled", false))
				services.AddDbContextWithWolverineIntegration<ProductDbContext>(optionsAction);
			else
				services.AddDbContext<ProductDbContext>(optionsAction);



			var secretKey = configuration["Jwt:Key"];



			if (string.IsNullOrEmpty(secretKey))
				secretKey = Environment.GetEnvironmentVariable("JWT_KEY");

			if (string.IsNullOrEmpty(secretKey))
				throw new InvalidOperationException("JWT Key is not configured.");

			var issuer = configuration["Jwt:Issuer"];
			if (string.IsNullOrEmpty(issuer))
				issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");

			var audience = configuration["Jwt:Audience"];
			if (string.IsNullOrEmpty(audience))
				audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");


			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
						.GetBytes(secretKey)),
						ValidateIssuer = true,
						ValidIssuer = issuer,
						ValidateAudience = true,
						ValidAudience = audience,
					};
				});

			services.AddAuthorization();

			services.AddScoped<IProductRepository, ProductRepository>();
			services.AddScoped<ICategoryRepository, CategoryRepository>();
			// ── Caching ──────────────────────────────────────────────────────────
        services.AddMemoryCache();
        services.AddIdempotencyContextPropagation();



			// ── Messaging (MassTransit + RabbitMQ) ────────────────────────────────
			if (configuration.GetValue("RabbitMq:Enabled", false))
			{
				services.AddMassTransit(x =>
				{
					x.UsingRabbitMq((context, cfg) =>
					{
						var rabbitMqUrl = configuration["RabbitMq:Url"];
						if (Uri.TryCreate(rabbitMqUrl, UriKind.Absolute, out var rabbitUri)
							&& !rabbitMqUrl!.Contains("RABBITMQ_", StringComparison.OrdinalIgnoreCase))
						{
							cfg.Host(rabbitUri, h =>
							{
								h.Username(configuration["RabbitMq:Username"] ?? string.Empty);
								h.Password(configuration["RabbitMq:Password"] ?? string.Empty);
							});
							cfg.ConfigureEndpoints(context);
							return;
						}

						var rabbitMqHost = configuration["RabbitMq:Host"] ?? "localhost";
						var rabbitMqUser = configuration["RabbitMq:Username"]
							?? throw new InvalidOperationException("RabbitMq:Username is required.");
						var rabbitMqPass = configuration["RabbitMq:Password"]
							?? throw new InvalidOperationException("RabbitMq:Password is required.");
						cfg.Host(rabbitMqHost, h => { h.Username(rabbitMqUser); h.Password(rabbitMqPass); });
						cfg.ConfigureEndpoints(context);
					});
				});
			}

		
			//services.Scan(scan => scan
			//			.FromAssemblies(typeof(InfrastructureAssemplyMarker)
			//			.Assembly)
			//			.AddClasses(classes => classes
			//				.Where(type => type.Name.EndsWith("Client") && !type.IsAbstract && !type.IsGenericType))
			//			.AsImplementedInterfaces()
			//			.WithScopedLifetime()
			//		);

			services.AddBusinessIdempotency();
		return services;
		}
	}
}

