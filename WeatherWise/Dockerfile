# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for better caching
COPY ["WeatherWise.Api/WeatherWise.Api.csproj", "WeatherWise.Api/"]
COPY ["WeatherWise.Core/WeatherWise.Core.csproj", "WeatherWise.Core/"]
COPY ["WeatherWise.Infrastructure/WeatherWise.Infrastructure.csproj", "WeatherWise.Infrastructure/"]
RUN dotnet restore "WeatherWise.Api/WeatherWise.Api.csproj"

# Copy everything else
COPY . .
WORKDIR "/src/WeatherWise.Api"
RUN dotnet build "WeatherWise.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WeatherWise.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Copy published files
COPY --from=publish /app/publish .

# Use non-root user for security
USER $APP_UID

ENTRYPOINT ["dotnet", "WeatherWise.Api.dll"]