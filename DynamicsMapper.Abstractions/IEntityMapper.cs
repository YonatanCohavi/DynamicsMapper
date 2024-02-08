using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DynamicsMapper.Abstractions
{
    public interface IEntityMapper<T>
        where T : class
    {
        string Entityname { get; }
        ColumnSet Columns { get; }

        T Map(Entity entity, DynamicsMappingsTargets? dynamicMappingsTargets);
        T? Map(Entity entity, string alias, DynamicsMappingsTargets? dynamicMappingsTargets);
        Entity Map(T model, DynamicsMappingsTargets? dynamicMappingsTargets);
    }
}
