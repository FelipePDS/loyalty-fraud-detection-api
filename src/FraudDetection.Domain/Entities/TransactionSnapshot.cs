using FraudDetection.Domain.Common;
using FraudDetection.Domain.Enums;

namespace FraudDetection.Domain.Entities;

/// <summary>
/// An immutable local copy of a Loyalty API point transaction, ingested for analysis.
/// Mirrors <c>PointTransaction</c> from the Loyalty API — kept intentionally flat so the
/// fraud service has no compile-time dependency on the source domain.
/// </summary>
public sealed class TransactionSnapshot : Entity
{
    // Private parameterless constructor for EF Core.
    private TransactionSnapshot() { }

    public TransactionSnapshot(
        Guid originalTransactionId,
        Guid customerId,
        int points,
        TransactionType type,
        string description,
        string? referenceId,
        DateTime? expiresAt,
        DateTime transactionCreatedAt,
        bool isReversed)
    {
        OriginalTransactionId = originalTransactionId;
        CustomerId = customerId;
        Points = points;
        Type = type;
        Description = description;
        ReferenceId = referenceId;
        ExpiresAt = expiresAt;
        TransactionCreatedAt = transactionCreatedAt;
        IsReversed = isReversed;
    }

    /// <summary>The primary key of the transaction in the Loyalty API.</summary>
    public Guid OriginalTransactionId { get; private set; }

    /// <summary>Customer who owns this transaction.</summary>
    public Guid CustomerId { get; private set; }

    /// <summary>Positive = credit (Earned, Adjusted); negative = debit (Redeemed, Expired).</summary>
    public int Points { get; private set; }

    public TransactionType Type { get; private set; }

    public string Description { get; private set; } = default!;

    /// <summary>Optional external reference, e.g., an order ID from a partner system.</summary>
    public string? ReferenceId { get; private set; }

    /// <summary>When the earned credit expires. Null for debits and non-expiring credits.</summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>When the transaction originally occurred in the Loyalty API (UTC).</summary>
    public DateTime TransactionCreatedAt { get; private set; }

    /// <summary>Whether the original transaction was reversed.</summary>
    public bool IsReversed { get; private set; }

    // Navigation property — alerts that reference this snapshot.
    public IReadOnlyCollection<FraudAlert> Alerts { get; private set; } = [];
}
