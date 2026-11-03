using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SessionLogger.Domain.Catches;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Files
{
    // A catch photo. Owned by its Catch - deleting the Catch deletes the File.
    public class File : AuditedAggregateRoot<int>
    {
        public File(int id) : base(id)
        {
        }

        public File()
        {
            DateUploaded = DateTime.Now;
        }

        [Required]
        public int CatchId { get; set; }

        [ForeignKey(nameof(CatchId))]
        public virtual Catch Catch { get; set; }

        // The image itself - not loaded with the File unless asked for (see FileContent).
        // Must be set on a File being created; the other details are always loaded.
        public virtual FileContent Content { get; set; }

        [MaxLength(255)]
        public string FileName { get; set; }

        [MaxLength(10)]
        public string Extension { get; set; }

        public long Size { get; set; }

        public DateTime DateUploaded { get; set; }
    }
}
