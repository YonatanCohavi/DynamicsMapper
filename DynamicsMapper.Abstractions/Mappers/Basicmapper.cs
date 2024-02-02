using Microsoft.Xrm.Sdk;

namespace DynamicsMapper.Abstractions.Mappers
{
    public static class BasicMapper
    {
        public static T MapToEntity<T>(T value) => value;
        public static T MapToModel<T>(Entity entity, string attribute) => entity.GetAttributeValue<T>(attribute);
    }
}
