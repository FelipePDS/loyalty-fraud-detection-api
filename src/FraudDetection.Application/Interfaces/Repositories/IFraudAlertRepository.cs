using FraudDetection.Domain.Entities;
using FraudDetection.Domain.Enums;

namespace FraudDetection.Application.Interfaces.Repositories;

/// <summary>
/// Persistence contract for <see cref="FraudAlert"/> entities.
/// </summary>
public interface IFraudAlertRepository
{
    Task<FraudAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of alerts filtered by optional criteria, ordered newest-first.
    /// </summary>
    Task<(IReadOnlyList<FraudAlert> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? customerId = null,
        AlertSeverity? severity = null,
        AlertStatus? status = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all open alerts for a customer created after <paramref name="since"/>.</summary>
    Task<IReadOnlyList<FraudAlert>> GetOpenByCustomerIdAsync(
        Guid customerId,
        DateTime since,
        CancellationToken cancellationToken = default);

    /// <summary>Returns counts of alerts grouped by <see cref="AlertSeverity"/> and <see cref="AlertStatus"/>.</summary>
    Task<IReadOnlyDictionary<(AlertSeverity Severity, AlertStatus Status), int>> GetDashboardCountsAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(FraudAlert alert, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<FraudAlert> alerts, CancellationToken cancellationToken = default);

    void Update(FraudAlert alert);
}
