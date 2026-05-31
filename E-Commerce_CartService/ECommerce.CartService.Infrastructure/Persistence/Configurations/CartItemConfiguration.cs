using ECommerce.CartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.CartService.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration
	: IEntityTypeConfiguration<CartItem>
{
	public void Configure(
		EntityTypeBuilder<CartItem> builder)
	{
		builder.HasKey(x => x.Id);

		builder.Property(x => x.ProductId)
			.IsRequired();

		builder.Property(x => x.Quantity)
			.IsRequired();

		builder.Property(x => x.AddedAt)
			.IsRequired();
	}
}