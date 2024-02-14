using SessionLogger.Sessions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Weathers
{
    public class Weather : AuditedAggregateRoot<int>

    {

        public Weather(int id) : base(id)
        {

        }

        public Weather()
        {
            
        }
        [MaxLength(255)]
        public string WeatherType { get; set; }

    }
}
