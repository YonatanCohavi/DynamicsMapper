using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DynamicsMapper.Abstractions
{
    public interface IEntityMapper<T>
        where T : class
    {
        string Entityname { get; }
        ColumnSet Columns { get; }

        T Map(Entity entity);
        T? Map(Entity entity, string alias);
        Entity Map(T model);
    }
}
