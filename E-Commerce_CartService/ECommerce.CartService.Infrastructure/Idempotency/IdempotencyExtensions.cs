using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.CartService.Infrastructure.Idempotency;

public static class IdempotencyExtensions
{
    public static IServiceCollection AddBusinessIdempotency(this IServiceCollection services)
    {
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();
        services.AddHostedService<IdempotencyCleanupService>();
        return services;
    }

    public static IApplicationBuilder UseBusinessIdempotency(this IApplicationBuilder app)
    {
        return app.UseMiddleware<BusinessIdempotencyMiddleware>();
    }
}
