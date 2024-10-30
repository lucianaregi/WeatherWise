using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeatherWise.Core.Interfaces;
using WeatherWise.Core.Settings;

namespace WeatherWise.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly ILogger<WeatherController> _logger;

        public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
        {
            _weatherService = weatherService;
            _logger = logger;   
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Controller is working!");
        }

        [HttpGet("test/{param}")]
        public IActionResult TestParam(string param)
        {
            return Ok($"Received parameter: {param}");
        }

        [HttpGet("config-test")]
        public IActionResult TestConfig([FromServices] IOptions<OpenWeatherSettings> settings)
        {
            var apiKey = settings.Value.ApiKey;
            return Ok(new
            {
                hasApiKey = !string.IsNullOrEmpty(apiKey),
                keyLength = apiKey?.Length ?? 0,
                baseUrl = settings.Value.BaseUrl
            });
        }

        [HttpGet("{city}")]
        public async Task<IActionResult> GetWeather(string city)
        {
            try
            {
                _logger.LogInformation($"Receiving request for city: {city}");
                var result = await _weatherService.GetCurrentWeatherAsync(city);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting weather for {city}: {ex.Message}");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}