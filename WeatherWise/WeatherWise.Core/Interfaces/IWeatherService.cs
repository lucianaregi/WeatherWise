using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherWise.Core.Models;

namespace WeatherWise.Core.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherData> GetCurrentWeatherAsync(string city);
    }
}
