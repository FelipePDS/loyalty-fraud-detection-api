using FraudDetection.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FraudDetection.Infrastructure.Persistence;

public class FraudDetectionDbContext : DbContext
{
    public FraudDetectionDbContext(DbContextOptions<FraudDetectionDbContext> options)
        : base(options)
    {
    }

    public DbSet<TransactionSnapshot> TransactionSnapshots => Set<TransactionSnapshot>();
    public DbSet<FraudAlert> FraudAlerts => Set<FraudAlert>();
    public DbSet<FraudReport> FraudReports => Set<FraudReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FraudDetectionDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
