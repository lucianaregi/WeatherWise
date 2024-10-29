# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto e restaurar dependências
COPY ["WeatherWise.Api/WeatherWise.Api.csproj", "WeatherWise.Api/"]
COPY ["WeatherWise.Core/WeatherWise.Core.csproj", "WeatherWise.Core/"]
COPY ["WeatherWise.Infrastructure/WeatherWise.Infrastructure.csproj", "WeatherWise.Infrastructure/"]
RUN dotnet restore "WeatherWise.Api/WeatherWise.Api.csproj"

# Copiar todo o código fonte
COPY . .

# Publicar a aplicação
WORKDIR "/src/WeatherWise.Api"
RUN dotnet publish "WeatherWise.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Configurar variáveis de ambiente para a aplicação
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Copiar arquivos publicados do estágio de build
COPY --from=build /app/publish .

# Definir o usuário não-root por segurança
USER $APP_UID

ENTRYPOINT ["dotnet", "WeatherWise.Api.dll"]