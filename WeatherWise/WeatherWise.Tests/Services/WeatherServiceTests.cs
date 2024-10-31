using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using WeatherWise.Core.Models;
using WeatherWise.Core.Settings;
using WeatherWise.Infrastructure.Services;
using System.Net;
using FluentAssertions;
using Moq.Protected;

namespace WeatherWise.Tests.Services
{
    public class WeatherServiceTests
    {
        private readonly Mock<ILogger<WeatherService>> _loggerMock;
        private readonly IMemoryCache _cache;
        private readonly OpenWeatherSettings _weatherSettings;
        private readonly CacheSettings _cacheSettings;
        private readonly HttpClient _httpClient;
        private readonly Mock<HttpMessageHandler> _httpHandlerMock;

        public WeatherServiceTests()
        {
            // Setup mocks e configurações
            _loggerMock = new Mock<ILogger<WeatherService>>();
            _cache = new MemoryCache(new MemoryCacheOptions());

            _weatherSettings = new OpenWeatherSettings
            {
                ApiKey = "test-api-key",
                BaseUrl = "https://api.openweathermap.org/data/2.5",
                Units = "metric"
            };

            _cacheSettings = new CacheSettings
            {
                AbsoluteExpirationMinutes = 30,
                SlidingExpirationMinutes = 10
            };

            _httpHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpHandlerMock.Object);
        }

        private WeatherService CreateService()
        {
            var weatherSettingsOptions = Options.Create(_weatherSettings);
            var cacheSettingsOptions = Options.Create(_cacheSettings);

            return new WeatherService(
                _httpClient,
                weatherSettingsOptions,
                cacheSettingsOptions,
                _loggerMock.Object,
                _cache);
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_ValidCity_ReturnsWeatherData()
        {
            // Arrange
            var city = "London";
            var expectedResponse = @"{
                ""name"": ""London"",
                ""weather"": [{
                    ""description"": ""clear sky""
                }],
                ""main"": {
                    ""temp"": 20.5,
                    ""feels_like"": 19.8,
                    ""humidity"": 65
                }
            }";

            SetupHttpHandlerMock(expectedResponse, HttpStatusCode.OK);
            var service = CreateService();

            // Act
            var result = await service.GetCurrentWeatherAsync(city);

            // Assert
            result.Should().NotBeNull();
            result.CityName.Should().Be("London");
            result.Main.Temperature.Should().Be(20.5);
            result.Main.FeelsLike.Should().Be(19.8);
            result.Main.Humidity.Should().Be(65);
            result.Weather.Should().HaveCount(1);
            result.Weather[0].Description.Should().Be("clear sky");
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_CachedData_ReturnsCachedResult()
        {
            // Arrange
            var city = "Paris";
            var cachedData = new WeatherData
            {
                CityName = city,
                Main = new WeatherData.WeatherMain
                {
                    Temperature = 25.0,
                    FeelsLike = 24.0,
                    Humidity = 60
                },
                Weather = new List<WeatherData.WeatherDescription>
                {
                    new WeatherData.WeatherDescription
                    {
                        Description = "sunny"
                    }
                }
            };

            var cacheKey = $"weather_{city.ToLowerInvariant()}";
            _cache.Set(cacheKey, cachedData, TimeSpan.FromMinutes(30));

            var service = CreateService();

            // Act
            var result = await service.GetCurrentWeatherAsync(city);

            // Assert
            result.Should().BeEquivalentTo(cachedData);

            // Verificar que a API não foi chamada
            _httpHandlerMock.Protected()
                .Verify("SendAsync", Times.Never(),
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_ApiError_ThrowsException()
        {
            // Arrange
            var city = "InvalidCity";
            var errorResponse = @"{
        ""cod"": ""404"",
        ""message"": ""city not found""
    }";

            SetupHttpHandlerMock(errorResponse, HttpStatusCode.NotFound);
            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => service.GetCurrentWeatherAsync(city));

            // Verificar a mensagem de erro externa
            exception.Message.Should().StartWith("Error fetching weather data");

            // Verificar a exceção interna
            exception.InnerException.Should().BeOfType<HttpRequestException>();
            exception.InnerException.Message.Should().Contain("404");
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound, "city not found")]
        [InlineData(HttpStatusCode.Unauthorized, "Invalid API key")]
        [InlineData(HttpStatusCode.ServiceUnavailable, "Service Unavailable")]
        public async Task GetCurrentWeatherAsync_DifferentApiErrors_ThrowsAppropriateException(
            HttpStatusCode statusCode,
            string errorMessage)
        {
            // Arrange
            var city = "TestCity";
            var errorResponse = $@"{{
        ""cod"": ""{(int)statusCode}"",
        ""message"": ""{errorMessage}""
    }}";

            SetupHttpHandlerMock(errorResponse, statusCode);
            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => service.GetCurrentWeatherAsync(city));

            // Verificações
            exception.Message.Should().StartWith("Error fetching weather data");
            exception.InnerException.Should().BeOfType<HttpRequestException>();
            exception.InnerException.Message.Should().Contain(statusCode.ToString("D"));
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_NetworkError_ThrowsException()
        {
            // Arrange
            var city = "London";
            _httpHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => service.GetCurrentWeatherAsync(city));

            exception.Message.Should().StartWith("Error fetching weather data");
            exception.InnerException.Should().BeOfType<HttpRequestException>();
            exception.InnerException.Message.Should().Be("Network error");
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_TimeoutError_ThrowsException()
        {
            // Arrange
            var city = "London";
            _httpHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new TimeoutException("Request timed out"));

            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => service.GetCurrentWeatherAsync(city));

            exception.Message.Should().StartWith("Error fetching weather data");
            exception.InnerException.Should().BeOfType<TimeoutException>();
            exception.InnerException.Message.Should().Be("Request timed out");
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_InvalidJson_ThrowsException()
        {
            // Arrange
            var city = "London";
            var invalidJson = "invalid json response";

            SetupHttpHandlerMock(invalidJson, HttpStatusCode.OK);
            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => service.GetCurrentWeatherAsync(city));

            exception.Message.Should().Contain("Error fetching weather data");
        }

        [Fact]
        public async Task GetCurrentWeatherAsync_VerifyCacheExpiration()
        {
            // Arrange
            var city = "Tokyo";
            var expectedResponse = @"{
                ""name"": ""Tokyo"",
                ""weather"": [{
                    ""description"": ""clear sky""
                }],
                ""main"": {
                    ""temp"": 28.5,
                    ""feels_like"": 27.8,
                    ""humidity"": 70
                }
            }";

            SetupHttpHandlerMock(expectedResponse, HttpStatusCode.OK);
            var service = CreateService();

            // Act
            var firstCall = await service.GetCurrentWeatherAsync(city);
            // Simular expiração do cache
            await Task.Delay(1);
            _cache.Remove($"weather_{city.ToLowerInvariant()}");
            var secondCall = await service.GetCurrentWeatherAsync(city);

            // Assert
            firstCall.Should().NotBeNull();
            secondCall.Should().NotBeNull();
            ReferenceEquals(firstCall, secondCall).Should().BeFalse();
        }

        private void SetupHttpHandlerMock(string content, HttpStatusCode statusCode)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            };

            _httpHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
        }
    }
}