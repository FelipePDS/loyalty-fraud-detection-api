using FraudDetection.Application.Features.Transactions.Ingest;
using FraudDetection.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FraudDetection.Infrastructure.BackgroundServices;

/// <summary>
/// Fallback ingestion path for when webhook deliveries from the Loyalty API are missed.
/// Periodically polls GET /api/points/transactions for activity since the last successful
/// poll and feeds the results through the same <see cref="IngestTransactionsCommand"/> the
/// webhook endpoint uses, so both paths share one dedup-by-transaction-ID rule.
///
/// Uses a dedicated DI scope per run so MediatR/DbContext are not shared across polls.
/// </summary>
public sealed class TransactionPollingService : BackgroundService
{
    // Re-poll slightly further back than the last successful run to tolerate clock drift
    // and out-of-order writes on the Loyalty API side. Overlap-induced duplicates are
    // absorbed by the ingestion command's dedup-by-transaction-ID logic.
    private static readonly TimeSpan OverlapWindow = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TransactionPollingService> _logger;
    private readonly TimeSpan _pollInterval;
    private readonly TimeSpan _initialLookback;

    private DateTime? _lastPolledAt;

    public TransactionPollingService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<TransactionPollingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _pollInterval = TimeSpan.FromSeconds(
            ParseIntOrDefault(configuration["LoyaltyApi:PollingIntervalSeconds"], 60));
        _initialLookback = TimeSpan.FromMinutes(
            ParseIntOrDefault(configuration["LoyaltyApi:PollingInitialLookbackMinutes"], 30));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "TransactionPollingService started. Polling every {IntervalSeconds}s.", _pollInterval.TotalSeconds);

        using var timer = new PeriodicTimer(_pollInterval);

        try
        {
            do
            {
                await PollOnceAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // Expected during host shutdown.
        }

        _logger.LogInformation("TransactionPollingService stopped.");
    }

    private async Task PollOnceAsync(CancellationToken cancellationToken)
    {
        var pollStartedAt = DateTime.UtcNow;
        var since = _lastPolledAt ?? pollStartedAt - _initialLookback;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var loyaltyApiClient = scope.ServiceProvider.GetRequiredService<ILoyaltyApiClient>();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var transactions = await loyaltyApiClient.GetRecentTransactionsAsync(since, cancellationToken);

            if (transactions.Count > 0)
            {
                var result = await sender.Send(new IngestTransactionsCommand(transactions), cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Polling fetched {Received} transaction(s) since {Since:O}: {Ingested} new, {Duplicate} already known.",
                        transactions.Count, since, result.Value.IngestedCount, result.Value.DuplicateCount);
                }
                else
                {
                    _logger.LogWarning(
                        "Polling fetched {Count} transaction(s) since {Since:O} but ingestion failed: {Error}",
                        transactions.Count, since, result.Error?.Message);
                }
            }
            else
            {
                _logger.LogDebug("Polling found no new transactions since {Since:O}.", since);
            }

            // Only advance the cursor after a successful round-trip, so a failed poll retries the same window.
            _lastPolledAt = pollStartedAt - OverlapWindow;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction polling run failed; will retry on the next interval.");
        }
    }

    private static int ParseIntOrDefault(string? value, int defaultValue)
        => int.TryParse(value, out var parsed) ? parsed : defaultValue;
}
