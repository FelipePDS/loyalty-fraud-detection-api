using Microsoft.EntityFrameworkCore;

namespace FraudDetection.Infrastructure.Persistence;

public class FraudDetectionDbContext : DbContext
{
    public FraudDetectionDbContext(DbContextOptions<FraudDetectionDbContext> options)
        : base(options)
    {
    }

    // TODO: Add DbSet<T> properties for domain entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FraudDetectionDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
