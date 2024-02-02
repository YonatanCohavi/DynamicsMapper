using Microsoft.Xrm.Sdk;

namespace DynamicsMapper.Abstractions.Mappers
{
    public static class OptionsetMapper
    {
        public static OptionSetValue? MapToEntity(int? value) => value.HasValue ? new OptionSetValue(value.Value) : null;
        public static int? MapToModel(Entity entity, string attribute) => entity.GetAttributeValue<OptionSetValue>(attribute)?.Value;
    }
}
