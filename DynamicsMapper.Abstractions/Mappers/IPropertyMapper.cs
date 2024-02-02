using Microsoft.Xrm.Sdk;

namespace DynamicsMapper.Abstractions.Mappers
{
    public interface IPropertyMapper<TModel, TEntity>
    {
        TEntity MapToEntity(TModel value);
        TModel MapToModel(Entity entity, string attribute);
    }
}
