using FraudDetection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FraudDetection.Infrastructure.Persistence.Configurations;

internal sealed class FraudReportConfiguration : IEntityTypeConfiguration<FraudReport>
{
    public void Configure(EntityTypeBuilder<FraudReport> builder)
    {
        builder.ToTable("FraudReports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.MarkdownContent)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(r => r.WindowFrom)
            .IsRequired();

        builder.Property(r => r.WindowTo)
            .IsRequired();

        builder.Property(r => r.GeneratedAt)
            .IsRequired();

        builder.Property(r => r.AlertCount)
            .IsRequired();

        builder.Property(r => r.TransactionCount)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        // Index supports deduplication check (FindCachedAsync) and listing by customer.
        builder.HasIndex(r => new { r.CustomerId, r.GeneratedAt })
            .HasDatabaseName("IX_FraudReports_CustomerId_GeneratedAt");

        // Index for date-range filtering on report listings.
        builder.HasIndex(r => r.GeneratedAt)
            .HasDatabaseName("IX_FraudReports_GeneratedAt");
    }
}
