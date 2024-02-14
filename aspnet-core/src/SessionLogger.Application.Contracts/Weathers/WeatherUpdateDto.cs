using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SessionLogger.Weathers
{
    public class WeatherUpdateDto
    {
        [MaxLength(255)]
        public string? WeatherType { get; set; }
    }
}
