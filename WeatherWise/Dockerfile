# See https://aka.ms/customizecontainer to learn how to customize your debug container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Configurações de ambiente recomendadas
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar arquivos de projeto
COPY ["WeatherWise.Api/WeatherWise.Api.csproj", "WeatherWise.Api/"]
COPY ["WeatherWise.Core/WeatherWise.Core.csproj", "WeatherWise.Core/"]
COPY ["WeatherWise.Infrastructure/WeatherWise.Infrastructure.csproj", "WeatherWise.Infrastructure/"]

# Restaurar dependências com log detalhado
RUN dotnet restore "WeatherWise.Api/WeatherWise.Api.csproj" --verbosity normal

# Copiar todo o código fonte
COPY . .

# Build e Publish
WORKDIR "/src/WeatherWise.Api"
RUN dotnet publish "WeatherWise.Api.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    --verbosity normal \
    /p:UseAppHost=false

# Final Stage
FROM base AS final
WORKDIR /app
COPY --from=builder /app/publish .
ENTRYPOINT ["dotnet", "WeatherWise.Api.dll"]