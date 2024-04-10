using SessionLogger.Contracts.Search;
using SessionLogger.Folders;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Search
{
    public interface ISearchableRepository<TEntity> : IRepository<TEntity, int> where TEntity : class, IEntity<int>
    {
        Task<IQueryable<TEntity>> BaseDbQuery();
        EntityInfo EntityInfo { get; }
        Task<SearchResult> Search(UserViewVM userView, bool idsOnly = false, int skip = 0, int take = 500, System.Linq.Expressions.Expression<System.Func<TEntity, bool>> baseFilter = null);
        Task<FolderInfoVM> GetFolderInfo();
        Task<List<UserViewListVM>> GetUserViews();
        Task<int> SaveUserViewAs(string json, bool isPublic);
        Task<int> SaveUserView(int userViewId, string json);
        Task<bool> DeleteUserView(int id);

        Task<UserViewVM> GetUserView(int id);

    }
}
