using Microsoft.Xrm.Sdk;
using System;

namespace DynamicsMapper.Abstractions.Mappers
{
    public interface ILookupMapper
    {
        EntityReference? MapToEntity(Guid? id, string target);
        Guid? MapToModel(Entity entity, string attribute);
    }
    public class LookupMapper : ILookupMapper
    {
        private static LookupMapper? _mapper;
        public static LookupMapper Instance => _mapper ??= new LookupMapper();
        private LookupMapper() { }
        public EntityReference? MapToEntity(Guid? id, string target) => id.HasValue ? new EntityReference(target, id.Value) : null;
        public Guid? MapToModel(Entity entity, string attribute) => entity.GetAttributeValue<EntityReference>(attribute)?.Id;

    }
}
