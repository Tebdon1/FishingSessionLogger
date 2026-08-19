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
using SessionLogger.Domain.SpeciesTypes;

namespace SessionLogger.Repositories
{
    public class EfCoreSpeciesRepository : SearchableRepository<SessionLoggerDbContext, Species>, ISpeciesRepository
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public EfCoreSpeciesRepository(
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
                    Id = "species",
                    EntityTypeLabel = "Species",
                    EntityTypeLabelPlural = "Species",
                    DbContextType = typeof(SessionLoggerDbContext),
                    SearchFieldNames = new[] { "Id" }.ToList(),
                    AllowViews = true,
                    EntityType = typeof(Species),
                    AdminPermission = SessionLoggerPermissions.SessionConfig.Admin,
                    CreatePermission = SessionLoggerPermissions.SessionConfig.Create,
                    ViewPermission = SessionLoggerPermissions.SessionConfig.Search,
                    EditPermission = SessionLoggerPermissions.SessionConfig.Edit,
                    DeletePermission = SessionLoggerPermissions.SessionConfig.Delete,
                };

                entityInfo.SearchFieldNames.Clear();
                entityInfo.SearchFieldNames.Add("Name");
                entityInfo.SearchFieldNames.Add("IsSaltwater");

                entityInfo.SortColumns.Clear();
                entityInfo.SortColumns.Add(new SortField
                {
                    FieldName = "Name"
                });

                return entityInfo;
            }
        }
    }
}
