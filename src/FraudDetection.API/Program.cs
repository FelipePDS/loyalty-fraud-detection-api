using FraudDetection.Application;
using FraudDetection.Infrastructure;
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

// ─── Middleware pipeline ─────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// ─── Endpoint registration ───────────────────────────────────────────────────
// TODO: register endpoint groups here, e.g.:
// app.MapFraudAlertEndpoints();

app.Run();
