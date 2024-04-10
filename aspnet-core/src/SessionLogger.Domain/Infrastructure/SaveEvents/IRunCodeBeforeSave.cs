using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionLogger.Infrastructure.SaveEvents
{
    public interface IRunCodeBeforeSave
    {
        void BeforeSave(EntityEntry entryChangeTracker, DbContext context);
    }
}
