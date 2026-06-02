using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.EmailService.Idempotency;

public class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.Key, x.OperationName }).IsUnique();
        builder.Property(x => x.Key).HasMaxLength(255).IsRequired();
        builder.Property(x => x.OperationName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
    }
}
