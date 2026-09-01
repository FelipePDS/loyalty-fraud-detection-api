# ──────────────────────────────────────────────────────────────────────────────
# Stage 1: Build
# ──────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files first for optimal layer caching.
COPY FraudDetection.slnx ./
COPY src/FraudDetection.Domain/FraudDetection.Domain.csproj src/FraudDetection.Domain/
COPY src/FraudDetection.Application/FraudDetection.Application.csproj src/FraudDetection.Application/
COPY src/FraudDetection.Infrastructure/FraudDetection.Infrastructure.csproj src/FraudDetection.Infrastructure/
COPY src/FraudDetection.API/FraudDetection.API.csproj src/FraudDetection.API/

# Restore dependencies
RUN dotnet restore src/FraudDetection.API/FraudDetection.API.csproj

# Copy source and build
COPY src/ src/
RUN dotnet build src/FraudDetection.API/FraudDetection.API.csproj -c Release --no-restore

# Publish
RUN dotnet publish src/FraudDetection.API/FraudDetection.API.csproj -c Release --no-build -o /app/publish

# ──────────────────────────────────────────────────────────────────────────────
# Stage 2: Runtime
# ──────────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Run as non-root user for security.
RUN addgroup --system --gid 1001 appgroup \
 && adduser  --system --uid 1001 --ingroup appgroup appuser
USER appuser

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FraudDetection.API.dll"]
