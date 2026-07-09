using FraudDetection.Domain.Entities;

namespace FraudDetection.Application.Interfaces.Repositories;

/// <summary>
/// Persistence contract for <see cref="FraudReport"/> entities.
/// </summary>
public interface IFraudReportRepository
{
    Task<FraudReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of reports, ordered newest-first.
    /// Optionally filtered by customer and date range of the report's generation time.
    /// </summary>
    Task<(IReadOnlyList<FraudReport> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? customerId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the most recent report that matches the same customer + window parameters
    /// and was generated after <paramref name="notBefore"/>.
    /// Used for cache-hit deduplication before calling the LLM.
    /// </summary>
    Task<FraudReport?> FindCachedAsync(
        Guid? customerId,
        DateTime windowFrom,
        DateTime windowTo,
        DateTime notBefore,
        CancellationToken cancellationToken = default);

    Task AddAsync(FraudReport report, CancellationToken cancellationToken = default);
}
