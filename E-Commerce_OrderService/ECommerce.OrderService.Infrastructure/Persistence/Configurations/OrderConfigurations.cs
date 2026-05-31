using ECommerce.OrderService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.OrderService.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ShippingAddress).IsRequired().HasMaxLength(500);
        builder.Property(x => x.TotalPrice).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.PaymentStatus).HasConversion<string>();

        builder.HasMany(x => x.Items)
               .WithOne()
               .HasForeignKey(x => x.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        // Optimistic concurrency
        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
    }
}
