using FraudDetection.Application.Interfaces.Repositories;
using FraudDetection.Infrastructure.Persistence;
using FraudDetection.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FraudDetection.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core DbContext with PostgreSQL + retry on transient failures.
        services.AddDbContext<FraudDetectionDbContext>((_, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("FraudDb"),
                npgsql =>
                {
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                    npgsql.MigrationsAssembly(typeof(FraudDetectionDbContext).Assembly.FullName);
                });
        });

        // Repositories — scoped to match DbContext lifetime.
        services.AddScoped<ITransactionSnapshotRepository, TransactionSnapshotRepository>();
        services.AddScoped<IFraudAlertRepository, FraudAlertRepository>();
        services.AddScoped<IFraudReportRepository, FraudReportRepository>();

        return services;
    }
}
