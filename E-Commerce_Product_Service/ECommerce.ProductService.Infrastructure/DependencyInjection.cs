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

			services.AddDbContextWithWolverineIntegration<ProductDbContext>(options =>
				options.UseSqlServer(connectionString));



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



			// ── Messaging (MassTransit + RabbitMQ) ────────────────────────────────
			services.AddMassTransit(x =>
			{
				x.UsingRabbitMq((context, cfg) =>
				{
					var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "localhost";
					var rabbitMqUser = configuration["RabbitMQ:Username"] ?? "guest";
					var rabbitMqPass = configuration["RabbitMQ:Password"] ?? "guest";

					cfg.Host(rabbitMqHost, h =>
					{
						h.Username(rabbitMqUser);
						h.Password(rabbitMqPass);
					});

					cfg.ConfigureEndpoints(context);
				});
			});

		
			//services.Scan(scan => scan
			//			.FromAssemblies(typeof(InfrastructureAssemplyMarker)
			//			.Assembly)
			//			.AddClasses(classes => classes
			//				.Where(type => type.Name.EndsWith("Client") && !type.IsAbstract && !type.IsGenericType))
			//			.AsImplementedInterfaces()
			//			.WithScopedLifetime()
			//		);

			return services;
		}
	}
}
