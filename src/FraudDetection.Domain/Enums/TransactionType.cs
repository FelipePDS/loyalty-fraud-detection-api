namespace FraudDetection.Domain.Enums;

/// <summary>
/// Mirrors TransactionType from the Loyalty API. Describes the direction and reason
/// for a point movement. Kept in sync to avoid translation errors during ingestion.
/// </summary>
public enum TransactionType
{
    Earned = 0,
    Redeemed = 1,
    Expired = 2,
    Adjusted = 3,
    Reversed = 4
}
