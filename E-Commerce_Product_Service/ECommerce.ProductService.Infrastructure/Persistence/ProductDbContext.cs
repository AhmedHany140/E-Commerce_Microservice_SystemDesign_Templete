using System.Reflection;
using ECommerce.ProductService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ProductService.Infrastructure.Persistence;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ECommerce.ProductService.Infrastructure.Idempotency.IdempotencyRecord> IdempotencyRecords => Set<ECommerce.ProductService.Infrastructure.Idempotency.IdempotencyRecord>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
