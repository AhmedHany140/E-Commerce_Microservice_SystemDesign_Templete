using ECommerce.CartService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.CartService.Infrastructure.Persistence.Configurations;

public class CartConfiguration
	: IEntityTypeConfiguration<Cart>
{
	public void Configure(
		EntityTypeBuilder<Cart> builder)
	{
		builder.HasKey(x => x.Id);

		builder.Property(x => x.UserId)
			.IsRequired()
			.HasMaxLength(100);

		builder.HasIndex(x => x.UserId)
			.IsUnique();

		builder.HasMany(x => x.Items)
			.WithOne(x => x.Cart)
			.HasForeignKey(x => x.CartId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}