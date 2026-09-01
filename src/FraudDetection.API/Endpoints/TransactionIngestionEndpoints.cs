using System.Text.Json;
using FraudDetection.Application.Common;
using FraudDetection.Application.Features.Transactions.Ingest;
using FraudDetection.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FraudDetection.API.Endpoints;

public static class TransactionIngestionEndpoints
{
    private const string SignatureHeaderName = "X-Webhook-Signature";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static IEndpointRouteBuilder MapTransactionIngestionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/transactions")
            .WithTags("Transactions");

        group.MapPost("/ingest", IngestAsync)
            .WithName("IngestTransactions")
            .WithSummary("Ingests a batch of Loyalty API transaction events")
            .WithDescription(
                "Accepts a JSON array of transaction events. " +
                "The X-Webhook-Signature header must contain sha256=<hex HMAC of the raw request body>.")
            .Produces<TransactionIngestionResult>(StatusCodes.Status202Accepted)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> IngestAsync(
        HttpRequest request,
        IWebhookSignatureValidator signatureValidator,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (request.ContentLength is 0)
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid payload",
                detail: "The request body is required.");

        await using var payloadStream = new MemoryStream();
        await request.Body.CopyToAsync(payloadStream, cancellationToken);
        var payload = payloadStream.ToArray();

        if (payload.Length == 0)
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid payload",
                detail: "The request body is required.");

        if (!signatureValidator.IsValid(payload, request.Headers[SignatureHeaderName]))
            return Results.Unauthorized();

        List<TransactionIngestionItem>? transactions;
        try
        {
            transactions = JsonSerializer.Deserialize<List<TransactionIngestionItem>>(payload, JsonOptions);
        }
        catch (JsonException)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid payload",
                detail: "The request body must be a valid JSON array of transaction events.");
        }

        if (transactions is null)
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid payload",
                detail: "The request body must be a JSON array of transaction events.");

        var result = await sender.Send(new IngestTransactionsCommand(transactions), cancellationToken);

        if (result.IsSuccess)
            return Results.Accepted(value: result.Value);

        return ToProblemResult(result.Error!);
    }

    private static IResult ToProblemResult(Error error)
    {
        if (error is ValidationError validationError)
        {
            return Results.ValidationProblem(
                validationError.Errors.ToDictionary(pair => pair.Key, pair => pair.Value),
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: validationError.Code,
                detail: validationError.Message);
        }

        return Results.Problem(
            statusCode: error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            },
            title: error.Code,
            detail: error.Message);
    }
}
