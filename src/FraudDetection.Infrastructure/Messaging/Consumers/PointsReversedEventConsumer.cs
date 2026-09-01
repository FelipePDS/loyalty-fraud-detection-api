using FraudDetection.Application.Features.Transactions.Ingest;
using FraudDetection.Application.IntegrationEvents;
using FraudDetection.Domain.Enums;
using MassTransit;
using MediatR;

namespace FraudDetection.Infrastructure.Messaging.Consumers;

/// <summary>Consumes <see cref="PointsReversedEvent"/> messages. See <see cref="PointsEarnedEventConsumer"/>.</summary>
public sealed class PointsReversedEventConsumer(ISender sender) : IConsumer<PointsReversedEvent>
{
    public async Task Consume(ConsumeContext<PointsReversedEvent> context)
    {
        var message = context.Message;

        var item = new TransactionIngestionItem(
            EventType: nameof(PointsReversedEvent),
            // The reversal itself is a new transaction in the Loyalty API, distinct from
            // the one it reverses — use its own ID as the snapshot's transaction ID and
            // link back to the original via ReferenceId.
            TransactionId: message.ReversalTransactionId,
            CustomerId: message.CustomerId,
            // The event only carries the magnitude (Math.Abs), not the direction of the
            // reversal, so the signed Points convention doesn't apply to this Reversed type.
            Points: message.Points,
            Type: TransactionType.Reversed,
            Description: $"Reversal of transaction {message.OriginalTransactionId} (event-driven ingestion)",
            ReferenceId: message.OriginalTransactionId.ToString(),
            ExpiresAt: null,
            CreatedAt: message.OccurredAt,
            IsReversed: true);

        await sender.Send(new IngestTransactionsCommand([item]), context.CancellationToken);
    }
}
