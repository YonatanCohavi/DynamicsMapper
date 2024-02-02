using Microsoft.Xrm.Sdk;
using System;

namespace DynamicsMapper.Abstractions.Mappers
{
    public interface ILookupMapper
    {
        EntityReference? MapToEntity(Guid? id, string target);
        Guid? MapToModel(Entity entity, string attribute);
    }
    public static class LookupMapper
    {
        public static EntityReference? MapToEntity(Guid? id, string target) => id.HasValue ? new EntityReference(target, id.Value) : null;
        public static Guid? MapToModel(Entity entity, string attribute) => entity.GetAttributeValue<EntityReference>(attribute)?.Id;

    }
}
