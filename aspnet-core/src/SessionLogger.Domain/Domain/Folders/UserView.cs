using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Folders
{
    public class UserView : FullAuditedAggregateRoot<int>
    {
        public UserView(int id) : base(id)
        {
        }

        public UserView()
        {
        }

        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string EntityType { get; set; }

        [MaxLength(50)]
        public string EntityInfoId { get; set; }

        [Required]
        public string UserViewJson { get; set; }

        public bool IsPublic { get; set; }
    }
}
