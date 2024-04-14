using SessionLogger.Domain.Baits;
using SessionLogger.Folders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionLogger.Baits
{
    public class BaitAppService : SearchableEntityAppService<Bait, BaitDto, BaitUpdateDto>
    {
        private IBaitRepository _baitRepository;
        public BaitAppService(IBaitRepository baitRepository) : base(baitRepository)
        {
            _baitRepository = baitRepository;
        }
    }
}
