namespace FraudDetection.Application.Interfaces;

/// <summary>
/// Commits changes made through application repositories as a single EF Core unit of work.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
