using FraudDetection.Application.Features.Transactions.Ingest;
using FraudDetection.Application.IntegrationEvents;
using FraudDetection.Domain.Enums;
using MassTransit;
using MediatR;

namespace FraudDetection.Infrastructure.Messaging.Consumers;

/// <summary>Consumes <see cref="PointsRedeemedEvent"/> messages. See <see cref="PointsEarnedEventConsumer"/>.</summary>
public sealed class PointsRedeemedEventConsumer(ISender sender) : IConsumer<PointsRedeemedEvent>
{
    public async Task Consume(ConsumeContext<PointsRedeemedEvent> context)
    {
        var message = context.Message;

        var item = new TransactionIngestionItem(
            EventType: nameof(PointsRedeemedEvent),
            TransactionId: message.TransactionId,
            CustomerId: message.CustomerId,
            // The event carries the redeemed amount as a positive magnitude; snapshots use
            // negative Points for debits.
            Points: -message.Points,
            Type: TransactionType.Redeemed,
            Description: "Points redeemed (event-driven ingestion)",
            ReferenceId: null,
            ExpiresAt: null,
            CreatedAt: message.OccurredAt,
            IsReversed: false);

        await sender.Send(new IngestTransactionsCommand([item]), context.CancellationToken);
    }
}
