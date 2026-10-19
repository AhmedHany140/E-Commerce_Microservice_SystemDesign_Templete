using ECommerce.CartService.Api.Grpc;
using ECommerce.CartService.Application.Common.Interfaces;
using ECommerce.CartService.Infrastructure.Clients;
using ECommerce.CartService.Infrastructure.Grpc;
using ECommerce.CartService.Infrastructure.Persistence;
using ECommerce.CartService.Infrastructure.Persistence.Repositories;
using ECommerce.CartService.Infrastructure.ProductClient;
using ECommerce.PaymentService.Api.Grpc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Wolverine.EntityFrameworkCore;
using ECommerce.CartService.Infrastructure.Idempotency;
using ECommerce.CartService.Infrastructure.Idempotency.Context;

namespace ECommerce.CartService.Infrastructure;

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
            services.AddDbContextWithWolverineIntegration<CartDbContext>(optionsAction);
        else
            services.AddDbContext<CartDbContext>(optionsAction);



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
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
					ValidateIssuer = true,
					ValidIssuer = issuer,
					ValidateAudience = true,
					ValidAudience = audience,
				};
			});

		services.AddAuthorization();

		services.AddScoped<ICartRepository, CartRepository>();

        // ── Caching ──────────────────────────────────────────────────────────
        services.AddMemoryCache();

        // ── gRPC client → Auth Service ────────────────────────────────────────
       
        var productServiceUrl =
            configuration["ProductService:GrpcUrl"];

		var orderServiceUrl =
		   configuration["OrderService:GrpcUrl"];


		var paymentServiceUrl =
		   configuration["PaymentService:GrpcUrl"];

		var authServiceUrl = configuration["AuthService:GrpcUrl"];

		services.AddGrpcClient<ProductGrpcService
            .ProductGrpcServiceClient>(options =>
		{
			options.Address = new Uri("https://localhost:7103");
		}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
		});


		services.AddGrpcClient<OrderGrpcService.OrderGrpcServiceClient>(options =>
		{
			options.Address = new Uri("https://localhost:7046");
		}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
		});

		
		services.AddGrpcClient<PaymentGrpcService.PaymentGrpcServiceClient
		>(options =>
		{
			options.Address = new Uri("https://localhost:7053");
		}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
		});





		services.Scan(scan => scan
					.FromAssemblies(typeof(InfrastructureAssemplyMarker)
					.Assembly)
					.AddClasses(classes => classes
						.Where(type => type.Name.EndsWith("Client") && !type.IsAbstract && !type.IsGenericType))
					.AsImplementedInterfaces()
					.WithScopedLifetime()
				);

        services.AddScoped<IPaymentServiceClient, PaymentClientService>();
		services.AddScoped<IProductServiceClient, ProductServiceClient>();
		services.AddScoped<IOrderServiceClient, OrderServiceClient>();

        services.AddIdempotencyContextPropagation();
        services.AddBusinessIdempotency();

        return services;
    }
}
