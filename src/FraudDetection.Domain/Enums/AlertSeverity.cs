namespace FraudDetection.Domain.Enums;

/// <summary>
/// Indicates the risk level of a detected fraud pattern.
/// Maps to LLM overall risk score: 0-25 Low, 26-50 Medium, 51-75 High, 76-100 Critical.
/// </summary>
public enum AlertSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
