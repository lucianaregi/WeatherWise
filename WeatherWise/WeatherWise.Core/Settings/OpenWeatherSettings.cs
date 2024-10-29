using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherWise.Core.Settings
{
    public class OpenWeatherSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5";
        public string Units { get; set; } = "metric"; // metric para Celsius
    }
}
