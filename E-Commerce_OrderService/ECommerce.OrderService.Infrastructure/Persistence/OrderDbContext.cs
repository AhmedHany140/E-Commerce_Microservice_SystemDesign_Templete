using ECommerce.OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.OrderService.Infrastructure.Persistence;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ECommerce.OrderService.Infrastructure.Idempotency.IdempotencyRecord> IdempotencyRecords => Set<ECommerce.OrderService.Infrastructure.Idempotency.IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
