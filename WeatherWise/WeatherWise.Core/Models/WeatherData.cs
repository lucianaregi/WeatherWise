using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WeatherWise.Core.Models
{
    public class WeatherData
    {
        [JsonPropertyName("main")]
        public WeatherMain Main { get; set; } = new();

        [JsonPropertyName("weather")]
        public List<WeatherDescription> Weather { get; set; } = new();

        [JsonPropertyName("name")]
        public string CityName { get; set; } = string.Empty;

        public class WeatherMain
        {
            [JsonPropertyName("temp")]
            public double Temperature { get; set; }

            [JsonPropertyName("feels_like")]
            public double FeelsLike { get; set; }

            [JsonPropertyName("humidity")]
            public int Humidity { get; set; }
        }

        public class WeatherDescription
        {
            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;
        }
    }
}
