using Microsoft.AspNetCore.Authorization;
using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.Folders;
using SessionLogger.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SessionLogger.SpeciesTypes
{
    public class SpeciesAppService : SearchableEntityAppService<Species, SpeciesDto, SpeciesUpdateDto>
    {
        private ISpeciesRepository _speciesRepository;
        public SpeciesAppService(ISpeciesRepository speciesRepository) : base(speciesRepository)
        {
            _speciesRepository = speciesRepository;
        }

        // Species is a shared, global list - only admins may add to or change it
        [Authorize(SessionLoggerPermissions.Lookups.Create)]
        public override Task<SaveResult<SpeciesDto>> CreateAsync(SpeciesUpdateDto input) => base.CreateAsync(input);

        [Authorize(SessionLoggerPermissions.Lookups.Edit)]
        public override Task<SaveResult<SpeciesDto>> UpdateAsync(int id, SpeciesUpdateDto input) => base.UpdateAsync(id, input);

        [Authorize(SessionLoggerPermissions.Lookups.Delete)]
        public override Task DeleteAsync(int id) => base.DeleteAsync(id);

        public async Task<PagedResultDto<SpeciesDto>> GetListAsync(PagedAndSortedResultRequestDto input)
        {
            var query = await _speciesRepository.GetQueryableAsync();
            
            var totalCount = query.Count();
            
            var items = query
                .OrderBy(x => x.Name)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .Select(x => new SpeciesDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsSaltwater = x.IsSaltwater,
                    CreationTime = x.CreationTime,
                    CreatorId = x.CreatorId,
                    LastModificationTime = x.LastModificationTime,
                    LastModifierId = x.LastModifierId
                })
                .ToList();

            return new PagedResultDto<SpeciesDto>(totalCount, items);
        }
    }
}
