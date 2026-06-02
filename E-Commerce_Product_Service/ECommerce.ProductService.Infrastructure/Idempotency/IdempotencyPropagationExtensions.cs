using System;
using ECommerce.ProductService.Infrastructure.Idempotency.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommerce.ProductService.Infrastructure.Idempotency;

public static class IdempotencyPropagationExtensions
{
    public static IServiceCollection AddIdempotencyContextPropagation(this IServiceCollection services)
    {
        services.TryAddSingleton<IIdempotencyContextAccessor, IdempotencyContextAccessor>();
        services.TryAddSingleton<IIdempotencyKeyProvider, IdempotencyKeyProvider>();
        services.AddScoped<IBusinessIdempotencyProcessor, BusinessIdempotencyProcessor>();
        
        services.AddTransient<GrpcClientIdempotencyInterceptor>();
        services.AddTransient<GrpcServerIdempotencyInterceptor>();
        
        return services;
    }
}
