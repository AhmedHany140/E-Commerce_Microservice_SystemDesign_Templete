using ECommerce.OrderService.Api.Grpc;
using ECommerce.OrderService.Application.Common.Interfaces;
using ECommerce.OrderService.Infrastructure.ExternalServices;
using ECommerce.OrderService.Infrastructure.Grpc;
using ECommerce.OrderService.Infrastructure.Messaging;
using ECommerce.OrderService.Infrastructure.Persistence;
using ECommerce.OrderService.Infrastructure.Persistence.Repositories;
using ECommerce.ProductService.Infrastructure.Grpc;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ECommerce.OrderService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistence
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString
            ("Constr")));

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



		services.AddScoped<IOrderRepository, OrderRepository>();

        // Cache
        services.AddMemoryCache();

     
        // HTTP Clients (Cart & Product Services)
        services.AddGrpcClient<CartGrpcService.CartGrpcServiceClient>(client =>
        {
            client.Address = new Uri(configuration["CartService:BaseUrl"]);
        });

        services.AddGrpcClient<ProductGrpcService.ProductGrpcServiceClient>(client =>
        {
            client.Address = new Uri(configuration["ProductService:BaseUrl"]);
        });

  

		services.Scan(scan => scan
					.FromAssemblies(typeof(ProductServiceClient)
					.Assembly)
					.AddClasses(classes => classes
						.Where(type => type.Name.EndsWith("Client") && !type.IsAbstract && !type.IsGenericType))
					.AsImplementedInterfaces()
					.WithScopedLifetime()
				);


		services.AddScoped<IEventBus, EventBus>();

        return services;
    }
}
