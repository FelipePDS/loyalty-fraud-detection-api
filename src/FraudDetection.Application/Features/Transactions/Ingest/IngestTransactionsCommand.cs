using FraudDetection.Application.Common;
using FraudDetection.Application.Interfaces;
using FraudDetection.Application.Interfaces.Repositories;
using FraudDetection.Domain.Entities;
using FraudDetection.Domain.Enums;
using MediatR;

namespace FraudDetection.Application.Features.Transactions.Ingest;

/// <summary>
/// Stores a batch of point-transaction events received from the Loyalty API.
/// </summary>
public sealed record IngestTransactionsCommand(
    IReadOnlyList<TransactionIngestionItem> Transactions)
    : ICommand<TransactionIngestionResult>;

/// <summary>
/// Transport-neutral representation of the Loyalty API transaction event contract.
/// </summary>
public sealed record TransactionIngestionItem(
    string EventType,
    Guid TransactionId,
    Guid CustomerId,
    int Points,
    TransactionType Type,
    string Description,
    string? ReferenceId,
    DateTime? ExpiresAt,
    DateTime CreatedAt,
    bool IsReversed);

/// <summary>Summary returned after accepting a transaction-ingestion batch.</summary>
public sealed record TransactionIngestionResult(
    int ReceivedCount,
    int IngestedCount,
    int DuplicateCount);

public sealed class IngestTransactionsCommandHandler(
    ITransactionSnapshotRepository transactionSnapshotRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<IngestTransactionsCommand, Result<TransactionIngestionResult>>
{
    public async Task<Result<TransactionIngestionResult>> Handle(
        IngestTransactionsCommand request,
        CancellationToken cancellationToken)
    {
        // Keep the first occurrence in the batch. Repeated deliveries are normal for webhooks.
        var distinctTransactions = request.Transactions
            .GroupBy(transaction => transaction.TransactionId)
            .Select(group => group.First())
            .ToList();

        var existingIds = await transactionSnapshotRepository.GetExistingOriginalTransactionIdsAsync(
            distinctTransactions.Select(transaction => transaction.TransactionId).ToArray(),
            cancellationToken);

        var snapshots = distinctTransactions
            .Where(transaction => !existingIds.Contains(transaction.TransactionId))
            .Select(transaction => new TransactionSnapshot(
                transaction.TransactionId,
                transaction.CustomerId,
                transaction.Points,
                transaction.Type,
                transaction.Description,
                transaction.ReferenceId,
                transaction.ExpiresAt,
                NormalizeUtc(transaction.CreatedAt),
                transaction.IsReversed))
            .ToList();

        if (snapshots.Count > 0)
        {
            await transactionSnapshotRepository.AddRangeAsync(snapshots, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new TransactionIngestionResult(
            request.Transactions.Count,
            snapshots.Count,
            request.Transactions.Count - snapshots.Count);
    }

    private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        _ => value.ToUniversalTime()
    };
}
