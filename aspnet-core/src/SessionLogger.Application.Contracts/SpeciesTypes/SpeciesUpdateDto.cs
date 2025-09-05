using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SessionLogger.SpeciesTypes;

public class SpeciesUpdateDto
{
    [MaxLength(255)]
    public string Name { get; set; }
    public bool IsSaltwater { get; set; }
}
