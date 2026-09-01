using FraudDetection.Application;
using FraudDetection.API.Endpoints;
using FraudDetection.Infrastructure;
using FraudDetection.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog ────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// ─── Infrastructure & Application layers ────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// ─── OpenAPI ────────────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply the schema prepared in phase 1 before accepting transaction snapshots.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FraudDetectionDbContext>();
    await dbContext.Database.MigrateAsync();
}

// ─── Middleware pipeline ─────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// ─── Endpoint registration ───────────────────────────────────────────────────
app.MapTransactionIngestionEndpoints();

app.Run();
