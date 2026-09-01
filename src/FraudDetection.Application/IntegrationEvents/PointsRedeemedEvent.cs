namespace FraudDetection.Application.IntegrationEvents;

/// <summary>Mirrors <c>LoyaltyApi.Domain.Events.PointsRedeemedEvent</c>. See <see cref="PointsEarnedEvent"/>.</summary>
public sealed record PointsRedeemedEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid CustomerId,
    int Points,
    Guid TransactionId);
