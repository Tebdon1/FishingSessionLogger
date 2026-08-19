using SessionLogger.Domain.SpeciesTypes;
using SessionLogger.Search;
using Volo.Abp.Domain.Repositories;

namespace SessionLogger.Domain.SpeciesTypes
{
    public interface ISpeciesRepository : ISearchableRepository<Species>
    {
    }
}

