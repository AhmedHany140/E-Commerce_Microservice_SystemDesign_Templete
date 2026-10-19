using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.Shared.ServiceDefaults;

namespace ECommerce.ProductService.Infrastructure.Idempotency;

public static class IdempotencyExtensions
{
    public static IServiceCollection AddBusinessIdempotency(this IServiceCollection services)
    {
        return services.AddSharedIdempotency<IIdempotencyStore, IdempotencyStore, IdempotencyCleanupService>();
    }

    public static IApplicationBuilder UseBusinessIdempotency(this IApplicationBuilder app)
    {
        return app.UseMiddleware<BusinessIdempotencyMiddleware>();
    }
}
