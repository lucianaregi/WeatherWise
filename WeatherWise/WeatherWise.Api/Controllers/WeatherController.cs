using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using WeatherWise.Core.Interfaces;
using WeatherWise.Core.Settings;
using System.Net;
using WeatherWise.Core.Models;

namespace WeatherWise.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _weatherService;
        private readonly ILogger<WeatherController> _logger;
        private const int MAX_CITY_LENGTH = 100;

        public WeatherController(IWeatherService weatherService, ILogger<WeatherController> logger)
        {
            _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("test")]
        [ResponseCache(Duration = 1800)]
        [SwaggerOperation(Summary = "Testa se o controller está funcionando.")]
        public IActionResult Test()
        {
            return Ok("Controller está funcionando!");
        }

        [HttpGet("test/{param}")]
        [SwaggerOperation(Summary = "Testa se o controller está recebendo um parâmetro.")]
        public IActionResult TestParam(string param)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                return BadRequest("O parâmetro não pode estar vazio.");
            }

            return Ok($"Parâmetro recebido: {param}");
        }

        [HttpGet("config-test")]
        [SwaggerOperation(Summary = "Testa se as configurações do OpenWeather estão corretas.")]
        public IActionResult TestConfig([FromServices] IOptions<OpenWeatherSettings> settings)
        {
            if (settings?.Value == null)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    "OpenWeather settings não está configurado");
            }

            var apiKey = settings.Value.ApiKey;
            return Ok(new
            {
                hasApiKey = !string.IsNullOrEmpty(apiKey),
                keyLength = apiKey?.Length ?? 0,
                baseUrl = settings.Value.BaseUrl
            });
        }

        [HttpGet("{city}")]
        [SwaggerOperation(Summary = "Obtém as informações meteorológicas para uma cidade específica.")]
        [ProducesResponseType(typeof(WeatherData), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.ServiceUnavailable)]
        public async Task<IActionResult> GetWeather(string city)
        {
            try
            {
                // Validação da entrada
                if (string.IsNullOrWhiteSpace(city))
                {
                    _logger.LogWarning("Tentativa de obter clima com nome da cidade vazia");
                    return BadRequest(new ErrorResponse("O nome da cidade não pode estar vazio"));
                }

                if (city.Length > MAX_CITY_LENGTH)
                {
                    _logger.LogWarning("Tentativa de obter clima com nome de cidade excedendo o comprimento máximo: {Length}", city.Length);
                    return BadRequest(new ErrorResponse($"O nome da cidade não pode exceder {MAX_CITY_LENGTH} caracteres"));
                }

                _logger.LogInformation("Recebendo solicitação para a cidade: {City}", city);
                var result = await _weatherService.GetCurrentWeatherAsync(city);
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro de comunicação com a API para a cidade: {City}", city);
                return StatusCode((int)HttpStatusCode.ServiceUnavailable,
                    new ErrorResponse("Weather service está fora"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter clima para a cidade: {City}", city);
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }


        [HttpGet("test-cache/{city}")]
        [SwaggerOperation(
        Summary = "Testa o cache do serviço de clima para uma cidade específica",
        Description = "Realiza duas chamadas consecutivas ao serviço e compara os resultados para verificar o funcionamento do cache")]
        [ProducesResponseType(typeof(CacheTestResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TestCache(string city)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(city))
                {
                    return BadRequest(new ErrorResponse("O nome da cidade não pode estar vazio"));
                }

                _logger.LogInformation("Testando o cache para a cidade: {City}", city);

                var result1 = await _weatherService.GetCurrentWeatherAsync(city);
                var result2 = await _weatherService.GetCurrentWeatherAsync(city);

                var cacheResult = new CacheTestResult
                {
                    FirstCall = result1,
                    SecondCall = result2,
                    AreSame = ReferenceEquals(result1, result2),
                    Timestamp = DateTime.UtcNow
                };

                return Ok(cacheResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao testar cache para a cidade: {City}", city);
                return BadRequest(new ErrorResponse(ex.Message));
            }
        }
    }

    
}