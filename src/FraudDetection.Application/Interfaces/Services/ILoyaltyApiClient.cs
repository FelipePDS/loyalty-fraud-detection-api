using FraudDetection.Application.Features.Transactions.Ingest;

namespace FraudDetection.Application.Interfaces.Services;

/// <summary>
/// Fetches point-transaction events directly from the Loyalty API. Used as a polling
/// fallback for transactions whose webhook delivery to <c>/api/transactions/ingest</c>
/// was missed or never sent.
/// </summary>
public interface ILoyaltyApiClient
{
    /// <summary>Returns transactions created at or after <paramref name="since"/> (UTC).</summary>
    Task<IReadOnlyList<TransactionIngestionItem>> GetRecentTransactionsAsync(
        DateTime since,
        CancellationToken cancellationToken = default);
}
