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
using SessionLogger.Domain.Baits;

namespace SessionLogger.Respositories
{
    public class EfCoreBaitRepository : SearchableRepository<SessionLoggerDbContext, Bait>, IBaitRepository
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public EfCoreBaitRepository(
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
                    Id = "bait",
                    EntityTypeLabel = "Bait",
                    EntityTypeLabelPlural = "Baits",
                    DbContextType = typeof(SessionLoggerDbContext),
                    SearchFieldNames = new[] { "Id" }.ToList(),
                    AllowViews = true,
                    EntityType = typeof(Bait),
                    AdminPermission = SessionLoggerPermissions.SessionConfig.Admin,
                    CreatePermission = SessionLoggerPermissions.SessionConfig.Create,
                    ViewPermission = SessionLoggerPermissions.SessionConfig.Search,
                    EditPermission = SessionLoggerPermissions.SessionConfig.Edit,
                    DeletePermission = SessionLoggerPermissions.SessionConfig.Delete,
                };

                entityInfo.SearchFieldNames.Clear();
                entityInfo.SearchFieldNames.Add("Name");

                entityInfo.SortColumns.Clear();
                entityInfo.SortColumns.Add(new SortField
                {
                    FieldName = "Name"
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
                        Title = "Name",
                        ColumnName="Name",
                        ColumnAlias = "name",
                        Sortable = true,
                        Width = 100,
                        Selected = true,
                        Description = "Name"
                    }
                };
                return entityInfo;
            }
        }
    }
}
