using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
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

        /// <summary>
        /// Testa se o controlador está funcionando.
        /// </summary>
        /// <returns>Mensagem de sucesso.</returns>
        [HttpGet("test")]
        [SwaggerOperation(Summary = "Testa se o controlador está funcionando.")]
        public IActionResult Test()
        {
            return Ok("Controller is working!");
        }

        /// <summary>
        /// Testa se o controlador está recebendo um parâmetro.
        /// </summary>
        /// <param name="param">Parâmetro de teste.</param>
        /// <returns>Mensagem com o parâmetro recebido.</returns>
        [HttpGet("test/{param}")]
        [SwaggerOperation(Summary = "Testa se o controlador está recebendo um parâmetro.")]
        public IActionResult TestParam(string param)
        {
            return Ok($"Received parameter: {param}");
        }

        /// <summary>
        /// Testa se as configurações do OpenWeather estão corretas.
        /// </summary>
        /// <param name="settings">Configurações do OpenWeather.</param>
        /// <returns>Informações sobre a chave da API e a URL base.</returns>
        [HttpGet("config-test")]
        [SwaggerOperation(Summary = "Testa se as configurações do OpenWeather estão corretas.")]
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

        /// <summary>
        /// Obtém as informações meteorológicas para uma cidade específica.
        /// </summary>
        /// <param name="city">Nome da cidade.</param>
        /// <returns>Dados meteorológicos da cidade.</returns>
        [HttpGet("{city}")]
        [SwaggerOperation(Summary = "Obtém as informações meteorológicas para uma cidade específica.")]
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