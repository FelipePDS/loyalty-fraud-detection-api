using System.Net.Http.Json;
using System.Text.Json;
using FraudDetection.Application.Features.Transactions.Ingest;
using FraudDetection.Application.Interfaces.Services;

namespace FraudDetection.Infrastructure.Services;

/// <summary>
/// Calls the Loyalty API's transaction-history endpoint over HTTP. Resilience (retry,
/// timeout, circuit breaking) is applied via the standard resilience handler attached
/// to this typed client's <see cref="HttpClient"/> in DI.
/// </summary>
internal sealed class HttpLoyaltyApiClient : ILoyaltyApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public HttpLoyaltyApiClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<IReadOnlyList<TransactionIngestionItem>> GetRecentTransactionsAsync(
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        var sinceUtc = since.Kind == DateTimeKind.Utc ? since : since.ToUniversalTime();
        var requestUri = $"api/points/transactions?since={Uri.EscapeDataString(sinceUtc.ToString("O"))}";

        using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var transactions = await response.Content
            .ReadFromJsonAsync<List<TransactionIngestionItem>>(JsonOptions, cancellationToken);

        return transactions ?? [];
    }
}
