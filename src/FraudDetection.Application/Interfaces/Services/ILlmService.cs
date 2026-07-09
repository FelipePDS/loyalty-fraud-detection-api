using FraudDetection.Application.DTOs;
using FraudDetection.Domain.Entities;

namespace FraudDetection.Application.Interfaces.Services;

/// <summary>
/// Abstraction over an LLM provider (e.g. OpenAI, Azure OpenAI) used for transaction
/// pattern analysis and natural language report generation.
/// </summary>
public interface ILlmService
{
    /// <summary>
    /// Sends the supplied transactions to the LLM and returns a structured analysis
    /// result describing detected suspicious patterns and an overall risk score.
    /// </summary>
    /// <param name="systemPrompt">
    /// The system prompt that instructs the model how to behave (role + output schema).
    /// </param>
    /// <param name="transactions">
    /// The customer's transaction history to analyse.
    /// </param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    Task<LlmAnalysisResult> AnalyzeTransactionsAsync(
        string systemPrompt,
        IReadOnlyList<TransactionSnapshot> transactions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a natural language fraud investigation report in Markdown format
    /// based on the supplied prompt containing alert and transaction context.
    /// </summary>
    /// <param name="prompt">The fully constructed prompt string for report generation.</param>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>The generated Markdown report as a plain string.</returns>
    Task<string> GenerateReportAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}
