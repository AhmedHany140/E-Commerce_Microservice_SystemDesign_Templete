using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using ECommerce.CartService.Infrastructure.Persistence;

namespace ECommerce.CartService.Infrastructure.Idempotency;

public class IdempotencyCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IdempotencyCleanupService> _logger;

    public IdempotencyCleanupService(IServiceProvider serviceProvider, ILogger<IdempotencyCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<CartDbContext>();

                var now = DateTime.UtcNow;
                var expiredRecords = await dbContext.IdempotencyRecords
                    .Where(r => r.ExpiresAt <= now)
                    .ToListAsync(stoppingToken);

                if (expiredRecords.Count > 0)
                {
                    dbContext.IdempotencyRecords.RemoveRange(expiredRecords);
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Cleaned up {Count} expired idempotency records.", expiredRecords.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up idempotency records.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}

