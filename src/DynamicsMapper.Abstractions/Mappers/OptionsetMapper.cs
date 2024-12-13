using Microsoft.Xrm.Sdk;

namespace DynamicsMapper.Abstractions.Mappers
{
    public sealed class OptionsetMapper : IOptionsetMapper
    {
        private static OptionsetMapper? _mapper;
        public static OptionsetMapper Instance => _mapper ??= new OptionsetMapper();
        private OptionsetMapper() { }
        public OptionSetValue? MapToEntity(int? value) => value.HasValue ? new OptionSetValue(value.Value) : null;
        public int? MapToModel(Entity entity, string attribute) => entity.GetAttributeValue<OptionSetValue>(attribute)?.Value;
    }

    public interface IOptionsetMapper
    {
        OptionSetValue? MapToEntity(int? value);
        int? MapToModel(Entity entity, string attribute);
    }
}
