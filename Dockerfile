FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first so dependency restore is cached separately from source changes.
COPY src/FraudDetection.Domain/FraudDetection.Domain.csproj src/FraudDetection.Domain/
COPY src/FraudDetection.Application/FraudDetection.Application.csproj src/FraudDetection.Application/
COPY src/FraudDetection.Infrastructure/FraudDetection.Infrastructure.csproj src/FraudDetection.Infrastructure/
COPY src/FraudDetection.API/FraudDetection.API.csproj src/FraudDetection.API/

RUN dotnet restore src/FraudDetection.API/FraudDetection.API.csproj

COPY src/ src/
RUN dotnet publish src/FraudDetection.API/FraudDetection.API.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FraudDetection.API.dll"]
