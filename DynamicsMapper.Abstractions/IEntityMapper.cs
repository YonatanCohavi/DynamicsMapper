using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DynamicsMapper.Abstractions
{
    public interface IEntityMapper<T>
        where T : class
    {
        string Entityname { get; }
        ColumnSet Columns { get; }

        T Map(Entity entity, DynamicsMapperSettings? settings = null);
        T? Map(Entity entity, string alias, DynamicsMapperSettings? settings = null);
        Entity Map(T model, DynamicsMappingsTargets? dynamicMappingsTargets, DynamicsMapperSettings? settings = null);
    }
}
