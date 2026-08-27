using System.ComponentModel.DataAnnotations;

namespace SessionLogger.Methods;

public class MethodUpdateDto
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }
}
