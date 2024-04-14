using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace SessionLogger.Domain.Files
{
    public class File : FullAuditedAggregateRoot<int>
    {
        // File information
        public byte[] FileData { get; set; }

        [MaxLength(255)]
        public string FileName { get; set; }

        public string FileDataSearch { get; set; }

        public DateTime DateUploaded { get; set; }
        public long Size { get; set; }
        [MaxLength(10)]
        public string Extension { get; set; }
        //
        // NB No need to record user uploaded by here as user information is recorded in parent Item
        //

        public bool IsArchived { get; set; }
        public DateTime DateArchived { get; set; }

        // Download monitoring
        public DateTime? FirstDownloaded { get; set; }
        public DateTime? LastDownloaded { get; set; }
        public int DownloadCount { get; set; }

        // call this when item is downloaded and then do a saveChanges()
        public void Downloaded()
        {
            this.LastDownloaded = DateTime.Now;
            if (!this.FirstDownloaded.HasValue)
            {
                this.FirstDownloaded = this.LastDownloaded;
            }
            ++this.DownloadCount;
        }

        public File()
        {
            this.DateUploaded = DateTime.Now;   // use DateTime.Now as in PL Item entity

            this.FirstDownloaded = null;
            this.LastDownloaded = null;
            this.DownloadCount = 0;
        }
    }
}
