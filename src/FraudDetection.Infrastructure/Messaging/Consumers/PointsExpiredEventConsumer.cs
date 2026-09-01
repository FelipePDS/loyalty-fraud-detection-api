using FraudDetection.Application.Features.Transactions.Ingest;
using FraudDetection.Application.IntegrationEvents;
using FraudDetection.Domain.Enums;
using MassTransit;
using MediatR;

namespace FraudDetection.Infrastructure.Messaging.Consumers;

/// <summary>Consumes <see cref="PointsExpiredEvent"/> messages. See <see cref="PointsEarnedEventConsumer"/>.</summary>
public sealed class PointsExpiredEventConsumer(ISender sender) : IConsumer<PointsExpiredEvent>
{
    public async Task Consume(ConsumeContext<PointsExpiredEvent> context)
    {
        var message = context.Message;

        var item = new TransactionIngestionItem(
            EventType: nameof(PointsExpiredEvent),
            // The event represents an aggregate expiration batch, not a single source
            // transaction, so there is no original transaction ID to key off. The event's
            // own ID is unique per occurrence and stands in as the dedup key.
            TransactionId: message.EventId,
            CustomerId: message.CustomerId,
            Points: -message.Points,
            Type: TransactionType.Expired,
            Description: $"{message.ExpiredCount} transaction(s) expired (event-driven ingestion)",
            ReferenceId: null,
            ExpiresAt: null,
            CreatedAt: message.OccurredAt,
            IsReversed: false);

        await sender.Send(new IngestTransactionsCommand([item]), context.CancellationToken);
    }
}
