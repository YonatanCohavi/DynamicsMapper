using Microsoft.Xrm.Sdk;

namespace DynamicsMapper.Abstractions.Mappers
{
    public class DynamicLookupTargetMapper : IDynamicLookupTargetMapper
    {
        private static DynamicLookupTargetMapper? _mapper;
        public static DynamicLookupTargetMapper Instance => _mapper ??= new DynamicLookupTargetMapper();
        private DynamicLookupTargetMapper() { }
        public string? MapToModel(Entity entity, string attribute)
        {
            var reference = entity.GetAttributeValue<EntityReference>(attribute);
            if (reference == null)
                return null;
            return reference.LogicalName;
        }
    }

    public interface IDynamicLookupTargetMapper
    {
        string? MapToModel(Entity entity, string attribute);
    }
}
