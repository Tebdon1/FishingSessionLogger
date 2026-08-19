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

        public byte[] FileData { get; set; }

        // Extracted text content, used by the generic full-text search infrastructure
        public string FileDataSearch { get; set; }

        [MaxLength(255)]
        public string FileName { get; set; }

        [MaxLength(10)]
        public string Extension { get; set; }

        public long Size { get; set; }

        public DateTime DateUploaded { get; set; }
    }
}
