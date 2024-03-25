using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SessionLogger.Baits
{
    public class BaitUpdateDto
    {
        [MaxLength(255)]
        public string? Name { get; set; }
    }
}
