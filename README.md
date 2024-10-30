# WeatherWise 🌦️

WeatherWise é uma aplicação web de previsão do tempo que utiliza a API OpenWeather para fornecer dados meteorológicos atualizados.

## 🏗️ Estrutura do Projeto

A solução está organizada seguindo os princípios de Clean Architecture:

```
WeatherWise/
├── WeatherWise.Api/           # Camada de apresentação e APIs
├── WeatherWise.Core/          # Camada de domínio e regras de negócio
└── WeatherWise.Infrastructure/# Camada de infraestrutura e serviços externos
```

### Componentes Principais

- **WeatherWise.Api**: Controladores e configurações da API
- **WeatherWise.Core**: Modelos, interfaces e regras de negócio
- **WeatherWise.Infrastructure**: Implementação de serviços externos

## 🚀 Tecnologias Utilizadas

- ASP.NET Core 8.0
- Docker e Docker Compose
- OpenWeather API
- Swagger/OpenAPI

## 📦 Configuração do Ambiente

### Pré-requisitos

- .NET 8.0 SDK
- Docker Desktop
- Visual Studio 2022 ou VSCode
- Chave de API do OpenWeather

### Configuração da API OpenWeather

1. Crie uma conta em [OpenWeather](https://openweathermap.org/)
2. Obtenha sua chave API
3. Configure no User Secrets:

```json
{
  "OpenWeather": {
    "ApiKey": "sua_chave_aqui",
    "BaseUrl": "https://api.openweathermap.org/data/2.5",
    "Units": "metric"
  }
}
```

## 🛠️ Configuração do Projeto

### Configuração do Docker

O projeto inclui Dockerfile e docker-compose para containerização:

```dockerfile
# Dockerfile principal
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# ... configurações do container
```

```yaml
# docker-compose.yml
services:
  weatherwise.api:
    container_name: weatherwise-api
    # ... configurações do serviço
```

### Estrutura da API

```csharp
public interface IWeatherService
{
    Task<WeatherData> GetCurrentWeatherAsync(string city);
}
```

### Endpoints Disponíveis

- `GET /Weather/{city}` - Obtém dados meteorológicos para uma cidade
- `GET /Weather/test` - Endpoint de teste
- `GET /Weather/config-test` - Teste de configuração

## 🚦 CORS

Configuração do CORS para desenvolvimento:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
```

## 🏃‍♂️ Executando o Projeto

### Usando Docker

```bash
# Construir e iniciar os containers
docker-compose up -d --build

# Verificar logs
docker logs weatherwise-api -f

# Parar os containers
docker-compose down
```

### Endpoints de Teste

- Swagger UI: `http://localhost:5000/swagger`
- API: `http://localhost:5000/Weather/london`

## 📝 Configurações Adicionais

### Logging

O projeto está configurado com logging detalhado para facilitar o debugging:

```csharp
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});
```

## 🔍 Monitoramento

- Logs disponíveis via Docker
- Swagger UI para documentação e teste da API
- Endpoints de diagnóstico para verificação de configuração

## 🔐 Segurança

- Configuração segura via User Secrets
- CORS configurado para desenvolvimento
- Container rodando com usuário não-root

## 🤝 Contribuindo

1. Faça um Fork do projeto
2. Crie sua Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a Branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está licenciado sob a Licença MIT.
