using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WeatherWise.Api.Controllers;
using WeatherWise.Core.Interfaces;
using WeatherWise.Core.Models;
using System.Net;
using static WeatherWise.Core.Models.WeatherData;

namespace WeatherWise.Tests.Controllers
{
    public class WeatherControllerTests
    {
        private readonly Mock<IWeatherService> _weatherServiceMock;
        private readonly Mock<ILogger<WeatherController>> _loggerMock;
        private readonly WeatherController _controller;

        public WeatherControllerTests()
        {
            _weatherServiceMock = new Mock<IWeatherService>();
            _loggerMock = new Mock<ILogger<WeatherController>>();
            _controller = new WeatherController(_weatherServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetWeather_ValidCity_ReturnsOkResult()
        {
            // Arrange
            var city = "Porto Alegre";
            var expectedWeather = new WeatherData
            {
                CityName = city,
                Main = new WeatherMain
                {
                    Temperature = 20.5,
                    FeelsLike = 19.8,
                    Humidity = 65
                },
                Weather = new List<WeatherDescription>
                {
                    new WeatherDescription
                    {
                        Description = "clear sky"
                    }
                }
            };

            _weatherServiceMock
                .Setup(x => x.GetCurrentWeatherAsync(city))
                .ReturnsAsync(expectedWeather);

            // Act
            var result = await _controller.GetWeather(city);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedWeather = Assert.IsType<WeatherData>(okResult.Value);

            // Verificações detalhadas
            returnedWeather.CityName.Should().Be(expectedWeather.CityName);
            returnedWeather.Main.Temperature.Should().Be(expectedWeather.Main.Temperature);
            returnedWeather.Main.FeelsLike.Should().Be(expectedWeather.Main.FeelsLike);
            returnedWeather.Main.Humidity.Should().Be(expectedWeather.Main.Humidity);
            returnedWeather.Weather.Should().HaveCount(1);
            returnedWeather.Weather.First().Description.Should().Be("clear sky");
        }

        
        [Fact]
        public async Task TestCache_ReturnsBothCallsAndComparisonResult()
        {
            // Arrange
            var city = "Porto Alegre";
            var weatherData = new WeatherData
            {
                CityName = city,
                Main = new WeatherMain
                {
                    Temperature = 20.5,
                    FeelsLike = 19.8,
                    Humidity = 65
                },
                Weather = new List<WeatherDescription>
            {
                new WeatherDescription
                {
                    Description = "clear sky"
                }
            }
            };

            _weatherServiceMock
                .Setup(x => x.GetCurrentWeatherAsync(city))
                .ReturnsAsync(weatherData);

            // Act
            var result = await _controller.TestCache(city);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var cacheResult = Assert.IsType<CacheTestResult>(okResult.Value);

            // Verificar estrutura básica
            cacheResult.Should().NotBeNull();
            cacheResult.FirstCall.Should().NotBeNull();
            cacheResult.SecondCall.Should().NotBeNull();
            cacheResult.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            // Verificar dados meteorológicos
            AssertWeatherData(cacheResult.FirstCall, weatherData);
            AssertWeatherData(cacheResult.SecondCall, weatherData);

            // Verificar chamadas ao serviço
            _weatherServiceMock.Verify(x => x.GetCurrentWeatherAsync(city), Times.Exactly(2));
        }

        private static void AssertWeatherData(WeatherData actual, WeatherData expected)
        {
            actual.CityName.Should().Be(expected.CityName);
            actual.Main.Should().NotBeNull();
            actual.Main.Temperature.Should().Be(expected.Main.Temperature);
            actual.Main.FeelsLike.Should().Be(expected.Main.FeelsLike);
            actual.Main.Humidity.Should().Be(expected.Main.Humidity);
            actual.Weather.Should().NotBeNull().And.HaveCount(1);
            actual.Weather.First().Description.Should().Be(expected.Weather.First().Description);
        }

        [Fact]
        public async Task GetWeather_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var city = "InvalidCity";
            var errorMessage = "Cidade não existe";

            _weatherServiceMock
                .Setup(x => x.GetCurrentWeatherAsync(city))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await _controller.GetWeather(city);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var error = badRequestResult.Value as ErrorResponse;
            Assert.Equal(errorMessage, error.Error);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GetWeather_EmptyCity_ReturnsBadRequest(string city)
        {
            // Act
            var result = await _controller.GetWeather(city);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var error = badRequestResult.Value as ErrorResponse;
            Assert.Contains("O nome da cidade não pode estar vazio", error.Error);
        }

        [Fact]
        public void Test_ReturnsOkWithCorrectMessage()
        {
            // Act
            var result = _controller.Test();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be("Controller está funcionando!");
        }

        [Theory]
        [InlineData("testParam")]
        [InlineData("123")]
        [InlineData("São Paulo")]
        public void TestParam_ReturnsOkWithParameter(string param)
        {
            // Act
            var result = _controller.TestParam(param);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be($"Parâmetro recebido: {param}");
        }

        [Fact]
        public async Task GetWeather_LogsInformation()
        {
            // Arrange
            var city = "Porto Alegre";
            _weatherServiceMock
                .Setup(x => x.GetCurrentWeatherAsync(city))
                .ReturnsAsync(new WeatherData
                {
                    CityName = city,
                    Main = new WeatherMain(),
                    Weather = new List<WeatherDescription>()
                });

            // Act
            await _controller.GetWeather(city);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Recebendo solicitação para a cidade: {city}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }
    }
}