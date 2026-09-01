namespace FraudDetection.Application.IntegrationEvents;

/// <summary>
/// Mirrors <c>LoyaltyApi.Domain.Events.PointsEarnedEvent</c>. Duplicated locally — rather
/// than referencing the Loyalty API's assembly — so the two services can evolve and deploy
/// independently, matching how <c>TransactionSnapshot</c> mirrors the source transaction shape.
/// </summary>
public sealed record PointsEarnedEvent(
    Guid EventId,
    DateTime OccurredAt,
    Guid CustomerId,
    int Points,
    Guid TransactionId);
