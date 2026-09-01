namespace FraudDetection.Application.Interfaces.Services;

/// <summary>
/// Validates the signature supplied by a trusted webhook sender.
/// </summary>
public interface IWebhookSignatureValidator
{
    bool IsValid(ReadOnlySpan<byte> payload, string? signature);
}
