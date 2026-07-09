using System.Diagnostics;
using FraudDetection.Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FraudDetection.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs the start, end, and duration of every request.
/// Reads the IsSuccess flag from Result/Result&lt;T&gt; responses to include in the log.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Handling {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        var isSuccess = response switch
        {
            Result result => result.IsSuccess,
            _ => true
        };

        logger.LogInformation(
            "Handled {RequestName} in {ElapsedMs}ms — Success: {IsSuccess}",
            requestName,
            stopwatch.ElapsedMilliseconds,
            isSuccess);

        return response;
    }
}
