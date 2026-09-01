using FraudDetection.Application.Interfaces;

namespace FraudDetection.Infrastructure.Persistence;

internal sealed class UnitOfWork(FraudDetectionDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
