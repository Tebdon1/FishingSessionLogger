using SessionLogger.Contracts.Search;
using SessionLogger.Domain.Folders;
using SessionLogger.EntityFrameworkCore;
using SessionLogger.Folders;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace SessionLogger.Search
{
    public abstract class SearchableRepository<TDbContext, TEntity> : EfCoreRepository<TDbContext, TEntity, int>, ISearchableRepository<TEntity> where TDbContext : IEfCoreDbContext
        where TEntity : class, IEntity<int>, IItem
    {
        private INoiseService _noiseService;
        private readonly IObjectMapper _objectMapper;
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<UserView, int> _userViewRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IPermissionChecker _permissionChecker;
        private readonly ICurrentPrincipalAccessor _currentPrincipleAccessor;
        //public string Id { get { return "product"; } }

         public SearchableRepository(
            ICurrentUser currentUser,
            IDbContextProvider<TDbContext> dbContextProvider,
            INoiseService noiseService,
            IObjectMapper objectMapper,
            IRepository<UserView, int> userViewRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IPermissionChecker permissionChecker,
            ICurrentPrincipalAccessor currentPrincipleAccessor
            )
            : base(dbContextProvider)
        {
            _currentUser = currentUser;
            _noiseService = noiseService;
            _objectMapper = objectMapper;
            _userViewRepository = userViewRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _permissionChecker = permissionChecker;
            _currentPrincipleAccessor = currentPrincipleAccessor;
        }

        public virtual EntityInfo EntityInfo
        {
            get
            {
                return new EntityInfo
                {
                    Id = "Item",
                    EntityTypeLabel = "Item",
                    DbContextType = typeof(SessionLoggerDbContext),
                    SearchFieldNames = new[] { "Id" }.ToList(),
                    AllowViews = true
                };
            }
        }

        public virtual async Task<IQueryable<TEntity>> BaseDbQuery()
        {
            return (await GetDbContextAsync()).Set<TEntity>();
        }

        public virtual async Task<SearchResult> Search(UserViewVM userView, bool idsOnly = false, int skip = 0, int take = 500, System.Linq.Expressions.Expression<System.Func<TEntity, bool>> baseFilter = null)
        {
            var baseQuery = await this.BaseDbQuery();

            if (baseFilter != null)
            {
                baseQuery = baseQuery.Where(baseFilter);
            }

            return await SearchHelper.Search<TEntity>(
                baseQuery: baseQuery,
                entityInfo: this.EntityInfo, 
                userView: userView, 
                idsOnly: idsOnly,
                skip: skip,
                take: take,
                noise: _noiseService.Noise());
        }

        public virtual async Task<FolderInfoVM> GetFolderInfo()
        {
            var folderInfo = _objectMapper.Map<EntityInfo, FolderInfoVM>(this.EntityInfo);

            folderInfo.UserView = SearchHelper.GetDefaultUserView(this.EntityInfo);

            if (_currentUser.IsAuthenticated)
            {
                folderInfo.UserCanAdmin = await _permissionChecker.IsGrantedAsync(_currentPrincipleAccessor.Principal, folderInfo.AdminPermission);
                folderInfo.UserCanCreate = await _permissionChecker.IsGrantedAsync(_currentPrincipleAccessor.Principal, folderInfo.CreatePermission);
                folderInfo.UserCanEdit = await _permissionChecker.IsGrantedAsync(_currentPrincipleAccessor.Principal, folderInfo.EditPermission);
                folderInfo.UserCanView = await _permissionChecker.IsGrantedAsync(_currentPrincipleAccessor.Principal, folderInfo.ViewPermission);
                folderInfo.UserCanDelete = await _permissionChecker.IsGrantedAsync(_currentPrincipleAccessor.Principal, folderInfo.DeletePermission);
            }
            else
            {
                folderInfo.UserCanView = string.IsNullOrEmpty(folderInfo.ViewPermission) || folderInfo.ViewPermission.ToLower() == "public";
            }

            return folderInfo;
        }

        public async Task<List<UserViewListVM>> GetUserViews()
        {
            var entityName = typeof(TEntity).Name;
            var entityInfoId = this.EntityInfo.Id;
            var userId = _currentUser.Id.Value;

            var userViewRepositoryQueryable = await _userViewRepository.GetQueryableAsync();

            return await (from v in userViewRepositoryQueryable
                          where v.EntityType == entityName && (v.IsPublic || v.CreatorId == userId) && (string.IsNullOrEmpty(v.EntityInfoId) || v.EntityInfoId == entityInfoId)
                         orderby v.Name 
                         select new UserViewListVM
                         {
                             Id = v.Id,
                             Name = v.Name == null ? v.Id.ToString() : v.Name
                         }).ToListAsync();
        }

        public async Task<UserViewVM> GetUserView(int userViewId)
        {
            var entityName = typeof(TEntity).Name;
            var userId = _currentUser.Id.Value;
            var entityInfoId = this.EntityInfo.Id;

            var userViewRepositoryQueryable = await _userViewRepository.GetQueryableAsync();


            var userView = await (from v in userViewRepositoryQueryable
                                  where v.Id == userViewId && v.EntityType == entityName && (v.IsPublic || v.CreatorId == userId)  && (string.IsNullOrEmpty(v.EntityInfoId) || v.EntityInfoId == entityInfoId)
                                  select v).FirstOrDefaultAsync();

            var userViewVM = JsonConvert.DeserializeObject<UserViewVM>(userView.UserViewJson);

            if (userViewVM.Query == null)
            {
                userViewVM.Query = new Query();
            }

            userViewVM.IsPublic = userView.IsPublic;
            userViewVM.UserCanEdit = await _permissionChecker.IsGrantedAsync(_currentPrincipleAccessor.Principal, this.EntityInfo.AdminPermission) || userView.CreatorId == userId;

            return userViewVM;
        }


        public async Task<int> SaveUserViewAs(string json, bool isPublic)
        {
            var vm = JsonConvert.DeserializeObject<UserViewVM>(json, GetJsonSerializerSettings());

            var userView = _objectMapper.Map<UserViewVM, UserView>(vm);

            var entityInfoId = this.EntityInfo.Id;

            userView.UserViewJson = json;

            userView = await _userViewRepository.InsertAsync(userView);

            userView.IsPublic = isPublic;
            userView.EntityInfoId = entityInfoId;

            await _unitOfWorkManager.Current.SaveChangesAsync();

            return userView.Id;
        }

        public async Task<bool> DeleteUserView(int userViewId)
        {
            var userId = _currentUser.Id.Value;
            var userView = await _userViewRepository.GetAsync(userViewId);

            if (await _permissionChecker.IsGrantedAsync(_currentPrincipleAccessor.Principal, this.EntityInfo.AdminPermission) || userView.CreatorId == userId)
            {
                await _userViewRepository.DeleteAsync(userViewId);
            }

            await _unitOfWorkManager.Current.SaveChangesAsync();

            return true;
        }

        public async Task<int> SaveUserView(int userViewId, string json)
        {
            var userId = _currentUser.Id.Value;

            var vm = JsonConvert.DeserializeObject<UserViewDto>(json, GetJsonSerializerSettings());

            var entityInfoId = this.EntityInfo.Id;

            bool isAdmin = await _permissionChecker.IsGrantedAsync(_currentPrincipleAccessor.Principal, this.EntityInfo.AdminPermission);

            var userView = await _userViewRepository.GetAsync(userViewId);

            if (userView != null && !(isAdmin || userView.CreatorId == userId))
            {
                //don't allow overwrite
                return 0;
            }

            userView.UserViewJson = json;
            userView.Name = vm.Name;
            userView.EntityInfoId = entityInfoId;

            userView = await _userViewRepository.UpdateAsync(userView);

            await _unitOfWorkManager.Current.SaveChangesAsync();

            return userView.Id;
        }

        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
        }

    }
}
