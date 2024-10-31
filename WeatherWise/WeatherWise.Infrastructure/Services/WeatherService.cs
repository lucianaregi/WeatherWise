using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;
        private readonly CacheSettings _cacheSettings;

        public WeatherService(
            HttpClient httpClient,
            IOptions<OpenWeatherSettings> settings,
            IOptions<CacheSettings> cacheSettings,
            ILogger<WeatherService> logger,
            IMemoryCache cache)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _cacheSettings = cacheSettings.Value;
            _logger = logger;
            _cache = cache;
        }

        public async Task<WeatherData> GetCurrentWeatherAsync(string city)
        {
            string cacheKey = $"weather_{city.ToLowerInvariant()}";

            try
            {
                // Tenta obter do cache
                if (_cache.TryGetValue(cacheKey, out WeatherData cachedWeather))
                {
                    _logger.LogInformation("Cache hit for city: {City}", city);
                    return cachedWeather;
                }

                // Se não estiver no cache, busca da API
                _logger.LogInformation("Cache miss for city: {City}. Fetching from API...", city);
                var weatherData = await FetchWeatherFromApiAsync(city);

                // Configura as opções do cache
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheSettings.AbsoluteExpirationMinutes))
                    .SetSlidingExpiration(TimeSpan.FromMinutes(_cacheSettings.SlidingExpirationMinutes))
                    .RegisterPostEvictionCallback((key, value, reason, state) =>
                    {
                        _logger.LogDebug("Cache entry {Key} was evicted. Reason: {Reason}", key, reason);
                    });

                // Armazena no cache
                _cache.Set(cacheKey, weatherData, cacheOptions);
                _logger.LogInformation("Data for city {City} cached successfully", city);

                return weatherData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing weather request for city: {City}", city);
                throw new Exception($"Error fetching weather data: {ex.Message}", ex);
            }
        }

        private async Task<WeatherData> FetchWeatherFromApiAsync(string city)
        {
            try
            {
                var url = $"{_settings.BaseUrl}/weather?q={city}&appid={_settings.ApiKey}&units={_settings.Units}";
                _logger.LogInformation("Calling OpenWeather API for city: {City}", city);

                var response = await _httpClient.GetAsync(url);
                _logger.LogInformation("API Response Status: {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("API Response Content: {Content}", content);

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
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error for city: {City}", city);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization error for city: {City}", city);
                throw;
            }
        }
    }
}