using Microsoft.Xrm.Sdk;

namespace DynamicsMapper.Abstractions.Mappers
{
    public class FormattedValuesMapper : IFormattedValuesMapper
    {
        private static FormattedValuesMapper? _mapper;
        public static FormattedValuesMapper Instance => _mapper ??= new FormattedValuesMapper();
        private FormattedValuesMapper() { }
        public string? MapToModel(Entity entity, string attribute)
        {
            if (entity.FormattedValues.TryGetValue(attribute, out var value))
                return value;
            return null;
        }
    }

    public interface IFormattedValuesMapper
    {
        string? MapToModel(Entity entity, string attribute);
    }
}
