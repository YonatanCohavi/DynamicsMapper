using Microsoft.Xrm.Sdk;

namespace DynamicsMapper.Abstractions.Mappers
{
    public class FormettedValuesMapper
    {
        public static string? MapToModel(Entity entity, string attribute)
        {
            if (entity.FormattedValues.TryGetValue(attribute, out var value))
                return value;
            return null;
        }
    }
}
