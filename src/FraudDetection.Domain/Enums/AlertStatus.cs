namespace FraudDetection.Domain.Enums;

/// <summary>
/// Lifecycle status of a fraud alert from initial detection through resolution.
/// </summary>
public enum AlertStatus
{
    /// <summary>Alert detected but not yet reviewed.</summary>
    Open = 0,

    /// <summary>Alert is under active investigation.</summary>
    Investigating = 1,

    /// <summary>Alert was confirmed as fraud and action was taken.</summary>
    Resolved = 2,

    /// <summary>Alert was reviewed and deemed a false positive.</summary>
    FalsePositive = 3
}
