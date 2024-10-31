using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WeatherWise.Core.Models
{
    public class CacheTestResult
    {
        /// <summary>
        /// Resultado da primeira chamada ao serviço
        /// </summary>
        [JsonPropertyName("firstCall")]
        public WeatherData FirstCall { get; set; }

        /// <summary>
        /// Resultado da segunda chamada ao serviço
        /// </summary>
        [JsonPropertyName("secondCall")]
        public WeatherData SecondCall { get; set; }

        /// <summary>
        /// Indica se as duas chamadas retornaram a mesma instância do objeto (cache hit)
        /// </summary>
        [JsonPropertyName("areSame")]
        public bool AreSame { get; set; }

        /// <summary>
        /// Timestamp do teste
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
