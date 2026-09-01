using System.Security.Cryptography;
using System.Text;
using FraudDetection.Application.Interfaces.Services;

namespace FraudDetection.Infrastructure.Services;

/// <summary>
/// Verifies SHA-256 HMAC signatures in the <c>sha256=&lt;hex&gt;</c> format.
/// </summary>
internal sealed class HmacWebhookSignatureValidator : IWebhookSignatureValidator
{
    private const string SignaturePrefix = "sha256=";
    private readonly byte[] _secret;

    public HmacWebhookSignatureValidator(string secret)
    {
        _secret = Encoding.UTF8.GetBytes(secret);
    }

    public bool IsValid(ReadOnlySpan<byte> payload, string? signature)
    {
        if (_secret.Length == 0 || string.IsNullOrWhiteSpace(signature))
            return false;

        var signatureValue = signature.Trim();
        if (!signatureValue.StartsWith(SignaturePrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        byte[] providedHash;
        try
        {
            providedHash = Convert.FromHexString(signatureValue[SignaturePrefix.Length..]);
        }
        catch (FormatException)
        {
            return false;
        }

        using var hmac = new HMACSHA256(_secret);
        var expectedHash = hmac.ComputeHash(payload.ToArray());

        return CryptographicOperations.FixedTimeEquals(expectedHash, providedHash);
    }
}
