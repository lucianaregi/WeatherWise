using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherWise.Core.Settings
{
    public class CacheSettings
    {
        public int AbsoluteExpirationMinutes { get; set; } = 30;
        public int SlidingExpirationMinutes { get; set; } = 10;
    }
}
