using FraudDetection.Domain.Common;
using FraudDetection.Domain.Enums;

namespace FraudDetection.Domain.Entities;

/// <summary>
/// Represents a suspicious pattern detected for a customer's transaction history.
/// Created by detection rules (rule-based or LLM-based) and progresses through a
/// review lifecycle tracked by <see cref="AlertStatus"/>.
/// </summary>
public sealed class FraudAlert : Entity
{
    // Private parameterless constructor for EF Core.
    private FraudAlert() { }

    public FraudAlert(
        Guid customerId,
        AlertSeverity severity,
        string patternType,
        string description,
        string detectionSource,
        IEnumerable<Guid> involvedTransactionIds)
    {
        CustomerId = customerId;
        Severity = severity;
        PatternType = patternType;
        Description = description;
        DetectionSource = detectionSource;
        Status = AlertStatus.Open;
        _involvedTransactionIds = [.. involvedTransactionIds];
    }

    /// <summary>Customer for whom the suspicious activity was detected.</summary>
    public Guid CustomerId { get; private set; }

    /// <summary>Risk level of the detected pattern.</summary>
    public AlertSeverity Severity { get; private set; }

    /// <summary>
    /// Short machine-readable label for the detected pattern type,
    /// e.g. "HighFrequency", "LargeRedemption", "LlmAnomaly".
    /// </summary>
    public string PatternType { get; private set; } = default!;

    /// <summary>Human-readable description of why this alert was raised.</summary>
    public string Description { get; private set; } = default!;

    /// <summary>
    /// Name of the rule or service that produced this alert,
    /// e.g. "HighFrequencyRule", "LlmAnomalyDetectionRule".
    /// </summary>
    public string DetectionSource { get; private set; } = default!;

    /// <summary>Current lifecycle status of the alert.</summary>
    public AlertStatus Status { get; private set; }

    /// <summary>Optional human-readable reason provided when the status was last changed.</summary>
    public string? StatusReason { get; private set; }

    /// <summary>When the status was last updated (UTC).</summary>
    public DateTime? StatusChangedAt { get; private set; }

    private readonly List<Guid> _involvedTransactionIds = [];

    /// <summary>IDs of the <see cref="TransactionSnapshot"/> records that triggered this alert.</summary>
    public IReadOnlyList<Guid> InvolvedTransactionIds => _involvedTransactionIds.AsReadOnly();

    /// <summary>
    /// Transitions the alert to a new status, recording the reason and timestamp.
    /// </summary>
    /// <param name="newStatus">The status to transition to.</param>
    /// <param name="reason">Optional reason explaining the status change.</param>
    /// <exception cref="ArgumentException">Thrown when transitioning to <see cref="AlertStatus.Open"/>.</exception>
    public void UpdateStatus(AlertStatus newStatus, string? reason = null)
    {
        if (newStatus == AlertStatus.Open)
            throw new ArgumentException("Cannot transition an alert back to Open.", nameof(newStatus));

        Status = newStatus;
        StatusReason = reason;
        StatusChangedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
