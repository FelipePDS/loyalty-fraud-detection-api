namespace FraudDetection.Application.IntegrationEvents;

/// <summary>
/// Mirrors <c>LoyaltyApi.Domain.Events.PointsExpiredEvent</c>. See <see cref="PointsEarnedEvent"/>.
/// Unlike the other point events, this one carries no source transaction ID — it represents
/// an aggregate batch of expired points for the customer, not a single transaction.
/// </summary>
public sealed record PointsExpiredEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid CustomerId,
    int Points,
    int ExpiredCount);
