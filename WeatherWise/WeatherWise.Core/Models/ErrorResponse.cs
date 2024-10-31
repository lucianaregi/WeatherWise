using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherWise.Core.Models
{
    public class ErrorResponse
    {
        public string Error { get; }
        public DateTime Timestamp { get; }

        public ErrorResponse(string message)
        {
            Error = message;
            Timestamp = DateTime.UtcNow;
        }
    }
}
