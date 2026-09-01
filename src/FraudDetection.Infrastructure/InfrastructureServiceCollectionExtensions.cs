using FraudDetection.Application.Interfaces;
using FraudDetection.Application.Interfaces.Repositories;
using FraudDetection.Application.Interfaces.Services;
using FraudDetection.Infrastructure.BackgroundServices;
using FraudDetection.Infrastructure.Messaging.Consumers;
using FraudDetection.Infrastructure.Persistence;
using FraudDetection.Infrastructure.Repositories;
using FraudDetection.Infrastructure.Services;
using MassTransit;
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IWebhookSignatureValidator>(_ =>
            new HmacWebhookSignatureValidator(configuration["LoyaltyApi:WebhookSecret"] ?? string.Empty));

        // Typed HTTP client for polling the Loyalty API — standard resilience handler adds
        // retry, timeout, and circuit-breaker policies around every call.
        services.AddHttpClient<ILoyaltyApiClient, HttpLoyaltyApiClient>(client =>
        {
            var baseUrl = configuration["LoyaltyApi:BaseUrl"]
                ?? throw new InvalidOperationException("LoyaltyApi:BaseUrl configuration is required.");
            client.BaseAddress = new Uri(baseUrl);
        })
        .AddStandardResilienceHandler();

        services.AddHostedService<TransactionPollingService>();

        // Event-driven fallback: consumes point-transaction domain events published by the
        // Loyalty API, as an alternative to the webhook and polling ingestion paths.
        services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();

            busConfigurator.AddConsumer<PointsEarnedEventConsumer>();
            busConfigurator.AddConsumer<PointsRedeemedEventConsumer>();
            busConfigurator.AddConsumer<PointsExpiredEventConsumer>();
            busConfigurator.AddConsumer<PointsReversedEventConsumer>();

            busConfigurator.UsingRabbitMq((context, rabbitConfigurator) =>
            {
                rabbitConfigurator.Host(
                    configuration["RabbitMQ:Host"] ?? "localhost",
                    "/",
                    hostConfigurator =>
                    {
                        hostConfigurator.Username(configuration["RabbitMQ:Username"] ?? "guest");
                        hostConfigurator.Password(configuration["RabbitMQ:Password"] ?? "guest");
                    });

                rabbitConfigurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
