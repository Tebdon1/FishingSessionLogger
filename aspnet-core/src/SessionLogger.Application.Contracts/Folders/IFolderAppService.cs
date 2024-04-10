using SessionLogger.Folders;
using SessionLogger.RemoteWebAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionLogger.WebInfo
{
    public interface IFolderAppService
    {
        Task<WebAPIResult<FolderInfoVM>> GetFolderInfo(string id);
    }
}
