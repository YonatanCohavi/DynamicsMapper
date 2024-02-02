using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace DynamicsMapper.Abstractions.Mappers
{
    public static class OptionSetValueCollectionMapper
    {
        public static OptionSetValueCollection? MapToEntity(int[]? values) => values is null ? null : new OptionSetValueCollection(values.Select(e => new OptionSetValue(e)).ToList());
        public static OptionSetValueCollection? MapToEntity<T>(T[]? values) where T : Enum
            => values is null ? null : new OptionSetValueCollection(values.Cast<int>().Select(e => new OptionSetValue(e)).ToList());
        public static int[]? MapToModel(Entity entity, string attribute)
            => entity.GetAttributeValue<OptionSetValueCollection>(attribute)?.Select(e => e.Value).ToArray();
        public static T[]? MapToModel<T>(Entity entity, string attribute) where T : Enum
            => entity.GetAttributeValue<OptionSetValueCollection>(attribute)?.Select(e => e.Value).Cast<T>().ToArray();
    }
}
