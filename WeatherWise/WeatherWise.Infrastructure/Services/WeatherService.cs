using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using WeatherWise.Core.Interfaces;
using WeatherWise.Core.Models;
using WeatherWise.Core.Settings;

namespace WeatherWise.Infrastructure.Services
{

    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenWeatherSettings _settings;
        private readonly ILogger<WeatherService> _logger;

        public WeatherService(
            HttpClient httpClient,
            IOptions<OpenWeatherSettings> settings,
            ILogger<WeatherService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<WeatherData> GetCurrentWeatherAsync(string city)
        {
            try
            {
                _logger.LogInformation($"Fetching weather data for city: {city}");
                var url = $"{_settings.BaseUrl}/weather?q={city}&appid={_settings.ApiKey}&units={_settings.Units}";
                _logger.LogInformation($"Calling OpenWeather API");

                var response = await _httpClient.GetAsync(url);
                _logger.LogInformation($"API Response Status: {response.StatusCode}");

                response.EnsureSuccessStatusCode();

                // Lê o conteúdo como string para logging
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"API Response Content: {content}");

                // Deserializa a resposta
                var weatherData = JsonSerializer.Deserialize<WeatherData>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (weatherData == null)
                {
                    throw new Exception("Failed to deserialize weather data");
                }

                return weatherData;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in WeatherService: {ex.Message}");
                throw new Exception($"Error fetching weather data: {ex.Message}");
            }
        }
    }
}