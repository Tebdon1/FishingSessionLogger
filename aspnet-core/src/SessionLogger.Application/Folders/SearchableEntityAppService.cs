using SessionLogger.Contracts.Search;
using SessionLogger.Domain;
using SessionLogger.RemoteWebAPI;
using SessionLogger.Search;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace SessionLogger.Folders
{
    public abstract class SearchableEntityAppService<TEntity, TReadDto, TCreateUpdateDto> : SessionLoggerAppService where TEntity : class, IEntity<int>
    {
        private readonly ISearchableRepository<TEntity> _repository;

        public SearchableEntityAppService(
            ISearchableRepository<TEntity> repository
        )
        {
            _repository = repository;
        }

        public async Task<WebAPIResult<FolderInfoVM>> GetFolderInfo()
        {
            var folderInfo = await _repository.GetFolderInfo();

            return new WebAPIResult<FolderInfoVM>
            {
                Error = false,
                Data = folderInfo
            };
        }

        public async Task<WebAPIResult<List<UserViewListVM>>> GetUserViews()
        {
            return new WebAPIResult<List<UserViewListVM>>
            {
                Error = false,
                Data = await _repository.GetUserViews()
            };
        }

        public class GetUserViewParams
        {
            public int UserViewId { get; set; }
        }

        public async Task<WebAPIResult<UserViewVM>> GetUserView(GetUserViewParams input)
        {
            return new WebAPIResult<UserViewVM>
            {
                Error = false,
                Data = await _repository.GetUserView(input.UserViewId)
            };
        }


        [HttpPost]
        public async Task<SearchResult> Search(SearchParams input)
        {
            var result = await _repository.Search(input.userView, input.idsOnly, input.skip, input.take, await BaseFilter());

            return new SearchResult
            {
                Data = result.Data,
                Total = result.Total
            };
        }

        public virtual async Task<System.Linq.Expressions.Expression<System.Func<TEntity, bool>>> BaseFilter()
        {
            return null;
        }

        public class SaveUserViewAsParams
        {
            public string Json { get; set; }
            public bool IsPublic { get; set; }
        }

        [HttpPost]
        public async Task<WebAPIResult<SaveUserViewResult>> SaveUserViewAs(SaveUserViewAsParams input)
        {
            var id = await _repository.SaveUserViewAs(json:input.Json, isPublic:input.IsPublic);

            return new WebAPIResult<SaveUserViewResult>
            {
                Error = false,
                Data = new SaveUserViewResult
                {
                    UserViewId = id
                }
            };
        }

        public class DeleteUserViewParams
        {
            public int UserViewId { get; set; }
        }

        [HttpPost]
        public async Task<WebAPIResult<bool>> DeleteUserView(DeleteUserViewParams input)
        {
            await _repository.DeleteUserView(input.UserViewId);

            return new WebAPIResult<bool>
            {
                Error = false,
                Data = true
            };
        }

        public class SaveUserViewParams
        {
            public int UserViewId { get; set; }
            public string Json { get; set; }
        }

        [HttpPost]
        public async Task<WebAPIResult<SaveUserViewResult>> SaveUserView(SaveUserViewParams input)
        {
            var id = await _repository.SaveUserView(input.UserViewId, input.Json);

            return new WebAPIResult<SaveUserViewResult>
            {
                Error = false,
                Data = new SaveUserViewResult
                {
                    UserViewId = id
                }
            };
        }

        public virtual async Task<TReadDto> GetAsync(int id)
        {
            return ObjectMapper.Map<TEntity, TReadDto>(await _repository.GetAsync(id));
        }

        public virtual async Task DeleteAsync(int id)
        {
            var cacheKey = typeof(TEntity).Name.ToLower() + "-" + id;
            await RemoveCache(cacheKey);
            cacheKey = typeof(TEntity).Name.ToLower() + "-*";
            await RemoveCache(cacheKey);
            var codeProperty = typeof(TReadDto).GetProperties().Where(x => x.Name == "Code").FirstOrDefault();
            if (codeProperty != null)
            {
                try
                {
                    var item = await this.GetAsync(id);
                    if (item != null)
                    {
                        string code = (string)codeProperty.GetValue(item);
                        cacheKey = typeof(TEntity).Name.ToLower() + "-" + code;
                        await RemoveCache(cacheKey);
                    }
                }
                catch { }
            }

            await _repository.DeleteAsync(id);
        }

        public virtual async Task<SaveResult<TReadDto>> CreateAsync(TCreateUpdateDto input)
        {
            var newItem = ObjectMapper.Map<TCreateUpdateDto, TEntity>(input);

            var syncDueProperty = typeof(TEntity).GetProperties().Where(x => x.Name == "SyncDue").FirstOrDefault();
            if (syncDueProperty != null)
            {
                syncDueProperty.SetValue(newItem, true);
            }

            var item = await _repository.InsertAsync(newItem);
            await CurrentUnitOfWork.SaveChangesAsync();

            var cacheKey = typeof(TEntity).Name.ToLower() + "-*";
            await RemoveCache(cacheKey);
            var codeProperty = typeof(TEntity).GetProperties().Where(x => x.Name == "Code").FirstOrDefault();
            if (codeProperty != null)
            {
                string code = (string)codeProperty.GetValue(item);
                cacheKey = typeof(TEntity).Name.ToLower() + "-" + code;
                await RemoveCache(cacheKey);
            }

            var value = ObjectMapper.Map<TEntity, TReadDto>(item);

            return new SaveResult<TReadDto>
            {
                Value = value
            };
        }

        public virtual async Task<TEntity> CreateEntityAsync(TEntity input)
        {
            var newItem = input;

            var syncDueProperty = typeof(TEntity).GetProperties().Where(x => x.Name == "SyncDue").FirstOrDefault();
            if (syncDueProperty != null)
            {
                syncDueProperty.SetValue(newItem, true);
            }

            var item = await _repository.InsertAsync(newItem);
            await CurrentUnitOfWork.SaveChangesAsync();

            var cacheKey = typeof(TEntity).Name.ToLower() + "-*";
            await RemoveCache(cacheKey);
            var codeProperty = typeof(TEntity).GetProperties().Where(x => x.Name == "Code").FirstOrDefault();
            if (codeProperty != null)
            {
                string code = (string)codeProperty.GetValue(item);
                cacheKey = typeof(TEntity).Name.ToLower() + "-" + code;
                await RemoveCache(cacheKey);
            }

            return item;
        }


        public virtual async Task<SaveResult<TReadDto>> UpdateAsync(int id, TCreateUpdateDto input)
        {
            var e = await _repository.GetAsync(id);
            ObjectMapper.Map(input, e);

            var syncDueProperty = typeof(TEntity).GetProperties().Where(x => x.Name == "SyncDue").FirstOrDefault();
            if (syncDueProperty != null)
            {
                syncDueProperty.SetValue(e, true);
            }

            TEntity updated = await _repository.UpdateAsync(e);

            var cacheKey = typeof(TEntity).Name.ToLower() + "-" + id;
            await RemoveCache(cacheKey);
            cacheKey = typeof(TEntity).Name.ToLower() + "-*";
            await RemoveCache(cacheKey);
            var codeProperty = typeof(TEntity).GetProperties().Where(x => x.Name == "Code").FirstOrDefault();
            if (codeProperty != null)
            {
                string code = (string) codeProperty.GetValue(updated);
                cacheKey = typeof(TEntity).Name.ToLower() + "-" + code;
                RemoveCache(cacheKey); // no need to await this
            }

            e = await _repository.GetAsync(id);

            var value = ObjectMapper.Map<TEntity, TReadDto>(e);

            return new SaveResult<TReadDto>
            {
                Value = value
            }; 
        }

        public virtual async Task<TEntity> UpdateEntityAsync(int id, TEntity input)
        {
            var syncDueProperty = typeof(TEntity).GetProperties().Where(x => x.Name == "SyncDue").FirstOrDefault();
            if (syncDueProperty != null)
            {
                syncDueProperty.SetValue(input, true);
            }

            TEntity updated = await _repository.UpdateAsync(input);

            var cacheKey = typeof(TEntity).Name.ToLower() + "-" + id;
            await RemoveCache(cacheKey);
            cacheKey = typeof(TEntity).Name.ToLower() + "-*";
            await RemoveCache(cacheKey);
            var codeProperty = typeof(TEntity).GetProperties().Where(x => x.Name == "Code").FirstOrDefault();
            if (codeProperty != null)
            {
                string code = (string)codeProperty.GetValue(updated);
                cacheKey = typeof(TEntity).Name.ToLower() + "-" + code;
                RemoveCache(cacheKey); // no need to await this
            }

            TEntity e = await _repository.GetAsync(id);

            return e;
        }

        public async Task<bool> RemoveCache(string cacheKey)
        {
            Caching.Remove(cacheKey);

            return true;
        }
    }
}

