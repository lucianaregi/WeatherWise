# WeatherWise 🌦️
WeatherWise é uma aplicação web de previsão do tempo que utiliza a API OpenWeather para fornecer dados meteorológicos atualizados.

## 🚀 Status
[![WeatherWise CI](https://github.com/lucianaregi/weatherwise/actions/workflows/ci.yml/badge.svg)](https://github.com/lucianaregi/weatherwise/actions/workflows/ci.yml)

## 🏗️ Estrutura do Projeto
A solução está organizada seguindo os princípios de Clean Architecture:
```
WeatherWise/
├── .github/                   # Configurações do GitHub
│   └── workflows/            # Workflows do GitHub Actions
│       └── ci.yml           # Pipeline de CI/CD
├── WeatherWise.Api/          # Camada de apresentação e APIs
├── WeatherWise.Core/         # Camada de domínio e regras de negócio
└── WeatherWise.Infrastructure/# Camada de infraestrutura e serviços externos
```

### Componentes Principais
- **WeatherWise.Api**: Controladores e configurações da API
- **WeatherWise.Core**: Modelos, interfaces e regras de negócio
- **WeatherWise.Infrastructure**: Implementação de serviços externos
- **.github/workflows**: Configurações de CI/CD

## 🚀 Tecnologias Utilizadas
- ASP.NET Core 8.0
- Docker e Docker Compose
- OpenWeather API
- Swagger/OpenAPI
- GitHub Actions para CI/CD

## 📦 Configuração do Ambiente
### Pré-requisitos
- .NET 8.0 SDK
- Docker Desktop
- Visual Studio 2022 ou VSCode
- Chave de API do OpenWeather
- Acesso ao GitHub Actions (para CI/CD)

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

## ⚙️ CI/CD com GitHub Actions
O projeto utiliza GitHub Actions para automação de CI/CD. O pipeline está configurado em `.github/workflows/ci.yml`:

```yaml
name: WeatherWise CI
on:
  push:
    branches: 
      - main
      - develop
      - 'feature/**'
      - 'release/**'
      - 'hotfix/**'
  pull_request:
    branches: 
      - main
      - develop

jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Build
      run: dotnet build
    - name: Build Docker image
      run: docker build -t weatherwise:latest .
```

### Fluxo de Trabalho
1. **Branches Protegidas**:
   - `main`: Branch de produção
   - `develop`: Branch de desenvolvimento
   
2. **Processo de CI**:
   - Build automático
   - Construção de imagem Docker
   - Validação de pull requests

3. **Gatilhos do Pipeline**:
   - Push para main/develop
   - Pull requests para main/develop
   - Push em branches feature/release/hotfix

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
3. Certifique-se que o pipeline de CI está passando
4. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
5. Push para a Branch (`git push origin feature/AmazingFeature`)
6. Abra um Pull Request para a branch `develop`

## 📄 Licença
Este projeto está licenciado sob a Licença MIT.

