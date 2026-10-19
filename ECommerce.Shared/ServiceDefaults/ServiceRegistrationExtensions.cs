using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ECommerce.Shared.ServiceDefaults;

public static class ServiceRegistrationExtensions
{
    /// <summary>
    /// Common registration used by services that keep their own idempotency store.
    /// The persistence implementation remains service-owned, while its DI lifecycle
    /// and cleanup registration are defined once here.
    /// </summary>
    public static IServiceCollection AddSharedIdempotency<TContract, TStore, TCleanup>(
        this IServiceCollection services)
        where TContract : class
        where TStore : class, TContract
        where TCleanup : class, IHostedService
    {
        services.AddScoped<TContract, TStore>();
        services.AddHostedService<TCleanup>();
        return services;
    }
}
