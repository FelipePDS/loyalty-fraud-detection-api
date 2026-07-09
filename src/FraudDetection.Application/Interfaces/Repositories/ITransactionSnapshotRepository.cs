using FraudDetection.Domain.Entities;

namespace FraudDetection.Application.Interfaces.Repositories;

/// <summary>
/// Persistence contract for <see cref="TransactionSnapshot"/> records ingested from the Loyalty API.
/// </summary>
public interface ITransactionSnapshotRepository
{
    Task<TransactionSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Looks up a snapshot by the original transaction ID in the Loyalty API.</summary>
    Task<TransactionSnapshot?> GetByOriginalTransactionIdAsync(
        Guid originalTransactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all snapshots for a customer within the given UTC time window,
    /// ordered by <see cref="TransactionSnapshot.TransactionCreatedAt"/> descending.
    /// </summary>
    Task<IReadOnlyList<TransactionSnapshot>> GetByCustomerIdAsync(
        Guid customerId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the IDs of customers who have at least one snapshot that has not been
    /// analysed yet (i.e., ingested after <paramref name="since"/>).
    /// </summary>
    Task<IReadOnlyList<Guid>> GetCustomerIdsWithNewSnapshotsSinceAsync(
        DateTime since,
        CancellationToken cancellationToken = default);

    /// <summary>Returns true when a snapshot for <paramref name="originalTransactionId"/> already exists.</summary>
    Task<bool> ExistsAsync(Guid originalTransactionId, CancellationToken cancellationToken = default);

    Task AddAsync(TransactionSnapshot snapshot, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<TransactionSnapshot> snapshots, CancellationToken cancellationToken = default);
}
