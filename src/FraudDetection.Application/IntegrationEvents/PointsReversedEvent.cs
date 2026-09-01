namespace FraudDetection.Application.IntegrationEvents;

/// <summary>Mirrors <c>LoyaltyApi.Domain.Events.PointsReversedEvent</c>. See <see cref="PointsEarnedEvent"/>.</summary>
public sealed record PointsReversedEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid CustomerId,
    int Points,
    Guid ReversalTransactionId,
    Guid OriginalTransactionId);
