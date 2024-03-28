using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.Baits;

public class BaitUpdateDto
{
    [MaxLength(255)]
    public string Name { get; set; }

}
