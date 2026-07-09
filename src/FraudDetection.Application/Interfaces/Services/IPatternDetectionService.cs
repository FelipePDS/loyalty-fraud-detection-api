using FraudDetection.Domain.Entities;

namespace FraudDetection.Application.Interfaces.Services;

/// <summary>
/// Orchestrates all registered detection rules against a customer's recent transactions
/// and returns the list of <see cref="FraudAlert"/> entities that were generated and persisted.
/// </summary>
public interface IPatternDetectionService
{
    /// <summary>
    /// Runs all detection rules for <paramref name="customerId"/> over the configured
    /// analysis window and persists any new alerts to the database.
    /// </summary>
    /// <param name="customerId">The customer to analyse.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// The list of newly generated <see cref="FraudAlert"/> records.
    /// Returns an empty list when no suspicious patterns are detected.
    /// </returns>
    Task<IReadOnlyList<FraudAlert>> AnalyzeAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}
