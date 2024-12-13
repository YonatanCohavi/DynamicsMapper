using DynamicsMapper.Abstractions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DynamicsMapper.FastMappers
{
    public class FastModelMapper<T>(ColumnSet columnSet, Func<Entity, DynamicsMapperSettings, T> map, string entityName)
    {
        private readonly Func<Entity, DynamicsMapperSettings, T> _mapping = map;
        public ColumnSet ColumnSet { get; set; } = columnSet;
        public string EntityName { get; } = entityName;
        public T Map(Entity entity) => _mapping(entity, DynamicsMapperSettings.Default);
        public T Map(Entity entity, DynamicsMapperSettings settings) => _mapping(entity, settings);
    }
}
;