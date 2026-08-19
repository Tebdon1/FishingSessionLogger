using System.Collections.Generic;
using System.Linq;
using Volo.Abp.EntityFrameworkCore;
using SessionLogger.EntityFrameworkCore;
using SessionLogger.Search;
using SessionLogger.Contracts.Search;
using Volo.Abp.Users;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Domain.Repositories;
using SessionLogger.Domain.Folders;
using Volo.Abp.Uow;
using SessionLogger.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Security.Claims;
using SessionLogger.Domain;
using System.Threading.Tasks;
using System.Threading;
using SessionLogger.Domain.Sessions;
using SessionLogger.GridData;

namespace SessionLogger.Respositories
{
    public class EfCoreSessionRepository : SearchableRepository<SessionLoggerDbContext, Session>, ISessionRepository
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public EfCoreSessionRepository(
             ICurrentUser currentUser,
             IDbContextProvider<SessionLoggerDbContext> dbContextProvider,
             INoiseService noiseService,
             IObjectMapper objectMapper,
             IRepository<UserView, int> userViewRepository,
             IUnitOfWorkManager unitOfWorkManager,
             IPermissionChecker permissionChecker,
             ICurrentPrincipalAccessor currentPrincipleAccessor

             )
             : base(
                 currentUser,
                 dbContextProvider,
                 noiseService,
                 objectMapper,
                 userViewRepository,
                 unitOfWorkManager,
                 permissionChecker,
                 currentPrincipleAccessor
             )
        {
            _unitOfWorkManager = unitOfWorkManager;
        }

        public override EntityInfo EntityInfo
        {
            get
            {
                var entityInfo = new EntityInfo
                {
                    Id = "session",
                    EntityTypeLabel = "Session",
                    EntityTypeLabelPlural = "Sessions",
                    DbContextType = typeof(SessionLoggerDbContext),
                    SearchFieldNames = new[] { "Id" }.ToList(),
                    AllowViews = true,
                    EntityType = typeof(Session),
                    AdminPermission = SessionLoggerPermissions.SessionConfig.Admin,
                    CreatePermission = SessionLoggerPermissions.SessionConfig.Create,
                    ViewPermission = SessionLoggerPermissions.SessionConfig.Search,
                    EditPermission = SessionLoggerPermissions.SessionConfig.Edit,
                    DeletePermission = SessionLoggerPermissions.SessionConfig.Delete,
                };

                entityInfo.SearchFieldNames.Clear();
                entityInfo.SearchFieldNames.Add("StartDateTime");
                entityInfo.SearchFieldNames.Add("EndDateTime");
                entityInfo.SearchFieldNames.Add("Venue.Name");
                entityInfo.SearchFieldNames.Add("Duration");



                entityInfo.SortColumns.Clear();
                entityInfo.SortColumns.Add(new SortField
                {
                    FieldName = "StartDateTime"
                });

                entityInfo.ColumnDefinitions = new List<ColumnInfo>
                {
                    new ColumnInfo{
                        ColumnName="Id",
                        ColumnAlias = "id",
                        Hidden = true,
                        Fixed = true,
                    },
                    new ColumnInfo{
                        Title = "Start DateTime",
                        ColumnName="StartDateTime",
                        ColumnAlias = "startDateTime",
                        Sortable = true,
                        Width = 150,
                        Selected = true,
                        DataType = ColumnDataType.Date,
                        Format = "dd/MMM/yyyy HH:mm",
                        Description = "Session Start Date and Time"
                    },
                    new ColumnInfo{
                        Title = "End DateTime",
                        ColumnName="EndDateTime",
                        ColumnAlias = "endDateTime",
                        Sortable = true,
                        Width = 150,
                        Selected = true,
                        DataType = ColumnDataType.Date,
                        Format = "dd/MMM/yyyy HH:mm",
                        Description = "Session End Date and Time"
                    },
                    new ColumnInfo{
                        Title = "Venue",
                        ColumnName="Venue.Name",
                        ColumnAlias = "venue",
                        Sortable = true,
                        Width = 200,
                        Selected = true,
                        Description = "Venue"
                    },
                    new ColumnInfo{
                        Title = "Duration",
                        ColumnName="Duration",
                        ColumnAlias = "duration",
                        Sortable = true,
                        Width = 100,
                        Selected = true,
                        Description = "Duration"
                    },                    
                };
                return entityInfo;
            }
        }
    }
}
