using FraudDetection.Application.Features.Transactions.Ingest;
using FraudDetection.Application.IntegrationEvents;
using FraudDetection.Domain.Enums;
using MassTransit;
using MediatR;

namespace FraudDetection.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumes <see cref="PointsEarnedEvent"/> messages and routes them through the same
/// <see cref="IngestTransactionsCommand"/> the webhook and polling ingestion paths use, so
/// all three sources share one dedup-by-transaction-ID rule.
/// </summary>
public sealed class PointsEarnedEventConsumer(ISender sender) : IConsumer<PointsEarnedEvent>
{
    public async Task Consume(ConsumeContext<PointsEarnedEvent> context)
    {
        var message = context.Message;

        var item = new TransactionIngestionItem(
            EventType: nameof(PointsEarnedEvent),
            TransactionId: message.TransactionId,
            CustomerId: message.CustomerId,
            Points: message.Points,
            Type: TransactionType.Earned,
            Description: "Points earned (event-driven ingestion)",
            ReferenceId: null,
            ExpiresAt: null,
            CreatedAt: message.OccurredAt,
            IsReversed: false);

        await sender.Send(new IngestTransactionsCommand([item]), context.CancellationToken);
    }
}
