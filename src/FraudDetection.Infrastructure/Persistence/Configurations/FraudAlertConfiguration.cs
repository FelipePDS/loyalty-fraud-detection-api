using FraudDetection.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FraudDetection.Infrastructure.Persistence.Configurations;

internal sealed class FraudAlertConfiguration : IEntityTypeConfiguration<FraudAlert>
{
    public void Configure(EntityTypeBuilder<FraudAlert> builder)
    {
        builder.ToTable("FraudAlerts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.CustomerId)
            .IsRequired();

        builder.Property(a => a.Severity)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.PatternType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.DetectionSource)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.StatusReason)
            .HasMaxLength(1000);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // Map the private backing field to a jsonb column.
        builder.Property<List<Guid>>("_involvedTransactionIds")
            .HasColumnName("InvolvedTransactionIds")
            .HasColumnType("jsonb")
            .IsRequired()
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Indexes for common filter and lookup patterns.
        builder.HasIndex(a => a.CustomerId)
            .HasDatabaseName("IX_FraudAlerts_CustomerId");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_FraudAlerts_Status");

        builder.HasIndex(a => new { a.Severity, a.Status })
            .HasDatabaseName("IX_FraudAlerts_Severity_Status");

        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("IX_FraudAlerts_CreatedAt");
    }
}
