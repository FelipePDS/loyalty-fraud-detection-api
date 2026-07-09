namespace FraudDetection.Application.DTOs;

/// <summary>
/// Structured response returned by the LLM after analysing a customer's transactions.
/// Deserialised from the LLM's JSON-mode output.
/// </summary>
public sealed record LlmAnalysisResult
{
    /// <summary>Risk score from 0 (no concern) to 100 (critical fraud risk).</summary>
    public int OverallRiskScore { get; init; }

    /// <summary>Individual suspicious patterns identified by the model.</summary>
    public IReadOnlyList<SuspiciousPattern> SuspiciousPatterns { get; init; } = [];
}

/// <summary>
/// A single suspicious pattern identified by the LLM within a transaction set.
/// </summary>
public sealed record SuspiciousPattern
{
    /// <summary>
    /// Short machine-readable label matching the fraud taxonomy,
    /// e.g. "HighFrequency", "RapidEarnAndRedeem", "PointLaundering".
    /// </summary>
    public string PatternType { get; init; } = default!;

    /// <summary>Confidence score from 0.0 (uncertain) to 1.0 (certain).</summary>
    public double Confidence { get; init; }

    /// <summary>Human-readable explanation of why this pattern was flagged.</summary>
    public string Description { get; init; } = default!;

    /// <summary>
    /// IDs of the <see cref="Domain.Entities.TransactionSnapshot"/> records
    /// that contributed to this pattern.
    /// </summary>
    public IReadOnlyList<Guid> InvolvedTransactionIds { get; init; } = [];
}
