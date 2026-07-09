using FraudDetection.Domain.Entities;
using FraudDetection.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FraudDetection.Infrastructure.Persistence.Configurations;

internal sealed class TransactionSnapshotConfiguration : IEntityTypeConfiguration<TransactionSnapshot>
{
    public void Configure(EntityTypeBuilder<TransactionSnapshot> builder)
    {
        builder.ToTable("TransactionSnapshots");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.OriginalTransactionId)
            .IsRequired();

        builder.Property(t => t.CustomerId)
            .IsRequired();

        builder.Property(t => t.Points)
            .IsRequired();

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.ReferenceId)
            .HasMaxLength(200);

        builder.Property(t => t.TransactionCreatedAt)
            .IsRequired();

        builder.Property(t => t.IsReversed)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Unique index prevents duplicate ingestion for the same source transaction.
        builder.HasIndex(t => t.OriginalTransactionId)
            .IsUnique()
            .HasDatabaseName("UX_TransactionSnapshots_OriginalTransactionId");

        // Covering index for time-window queries per customer.
        builder.HasIndex(t => new { t.CustomerId, t.TransactionCreatedAt })
            .HasDatabaseName("IX_TransactionSnapshots_CustomerId_TransactionCreatedAt");

        // Index for finding customers with new snapshots (scheduled analysis).
        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("IX_TransactionSnapshots_CreatedAt");
    }
}
