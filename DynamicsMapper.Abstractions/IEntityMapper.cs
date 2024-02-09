using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DynamicsMapper.Abstractions
{
    public interface IEntityMapper<T>
        where T : class
    {
        string Entityname { get; }
        ColumnSet Columns { get; }

        #region To Model
        T Map(Entity entity);
        T? Map(Entity entity, string alias);
        T Map(Entity entity, DynamicsMapperSettings settings);
        T? Map(Entity entity, string alias, DynamicsMapperSettings settings);
        #endregion To Model

        #region To Entity
        Entity Map(T model, DynamicsMappingsTargets dynamicMappingsTargets, DynamicsMapperSettings settings);
        Entity Map(T model, DynamicsMappingsTargets dynamicMappingsTargets);
        Entity Map(T model, DynamicsMapperSettings settings);
        Entity Map(T model);
        #endregion To Entity
    }

}
