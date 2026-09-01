using FraudDetection.Application.Interfaces.Repositories;
using FraudDetection.Domain.Entities;
using FraudDetection.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FraudDetection.Infrastructure.Repositories;

internal sealed class TransactionSnapshotRepository : ITransactionSnapshotRepository
{
    private readonly FraudDetectionDbContext _context;

    public TransactionSnapshotRepository(FraudDetectionDbContext context) => _context = context;

    public async Task<TransactionSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.TransactionSnapshots
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<TransactionSnapshot?> GetByOriginalTransactionIdAsync(
        Guid originalTransactionId,
        CancellationToken cancellationToken = default)
        => await _context.TransactionSnapshots
            .FirstOrDefaultAsync(t => t.OriginalTransactionId == originalTransactionId, cancellationToken);

    public async Task<IReadOnlyList<TransactionSnapshot>> GetByCustomerIdAsync(
        Guid customerId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
        => await _context.TransactionSnapshots
            .AsNoTracking()
            .Where(t => t.CustomerId == customerId
                     && t.TransactionCreatedAt >= from
                     && t.TransactionCreatedAt <= to)
            .OrderByDescending(t => t.TransactionCreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetCustomerIdsWithNewSnapshotsSinceAsync(
        DateTime since,
        CancellationToken cancellationToken = default)
        => await _context.TransactionSnapshots
            .AsNoTracking()
            .Where(t => t.CreatedAt >= since)
            .Select(t => t.CustomerId)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsAsync(Guid originalTransactionId, CancellationToken cancellationToken = default)
        => await _context.TransactionSnapshots
            .AnyAsync(t => t.OriginalTransactionId == originalTransactionId, cancellationToken);

    public async Task<IReadOnlySet<Guid>> GetExistingOriginalTransactionIdsAsync(
        IReadOnlyCollection<Guid> originalTransactionIds,
        CancellationToken cancellationToken = default)
    {
        if (originalTransactionIds.Count == 0)
            return new HashSet<Guid>();

        var existingIds = await _context.TransactionSnapshots
            .AsNoTracking()
            .Where(t => originalTransactionIds.Contains(t.OriginalTransactionId))
            .Select(t => t.OriginalTransactionId)
            .ToListAsync(cancellationToken);

        return existingIds.ToHashSet();
    }

    public async Task AddAsync(TransactionSnapshot snapshot, CancellationToken cancellationToken = default)
        => await _context.TransactionSnapshots.AddAsync(snapshot, cancellationToken);

    public async Task AddRangeAsync(
        IEnumerable<TransactionSnapshot> snapshots,
        CancellationToken cancellationToken = default)
        => await _context.TransactionSnapshots.AddRangeAsync(snapshots, cancellationToken);
}
