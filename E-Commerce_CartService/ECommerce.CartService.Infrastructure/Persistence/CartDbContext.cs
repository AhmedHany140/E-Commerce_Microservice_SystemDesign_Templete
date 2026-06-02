using System.Reflection;
using ECommerce.CartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.CartService.Infrastructure.Persistence;

public class CartDbContext : DbContext
{
    public CartDbContext(DbContextOptions<CartDbContext> options) : base(options)
    {
    }

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<ECommerce.CartService.Infrastructure.Idempotency.IdempotencyRecord> IdempotencyRecords => Set<ECommerce.CartService.Infrastructure.Idempotency.IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
