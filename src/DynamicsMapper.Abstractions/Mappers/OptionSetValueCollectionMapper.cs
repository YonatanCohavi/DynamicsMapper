using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace DynamicsMapper.Abstractions.Mappers
{
    public class OptionSetValueCollectionMapper : IOptionSetValueCollectionMapper
    {
        private static OptionSetValueCollectionMapper? _mapper;
        public static OptionSetValueCollectionMapper Instance => _mapper ??= new OptionSetValueCollectionMapper();
        private OptionSetValueCollectionMapper() { }
        public OptionSetValueCollection? MapToEntity(int[]? values) => values is null ? null : new OptionSetValueCollection(values.Select(e => new OptionSetValue(e)).ToList());
        public OptionSetValueCollection? MapToEntity<T>(T[]? values) where T : Enum
            => values is null ? null : new OptionSetValueCollection(values.Cast<int>().Select(e => new OptionSetValue(e)).ToList());
        public int[]? MapToModel(Entity entity, string attribute)
            => entity.GetAttributeValue<OptionSetValueCollection>(attribute)?.Select(e => e.Value).ToArray();
        public T[]? MapToModel<T>(Entity entity, string attribute) where T : Enum
            => entity.GetAttributeValue<OptionSetValueCollection>(attribute)?.Select(e => e.Value).Cast<T>().ToArray();
    }

    public interface IOptionSetValueCollectionMapper
    {
        OptionSetValueCollection? MapToEntity(int[]? values);
        OptionSetValueCollection? MapToEntity<T>(T[]? values) where T : Enum;
        int[]? MapToModel(Entity entity, string attribute);
        T[]? MapToModel<T>(Entity entity, string attribute) where T : Enum;
    }
}
