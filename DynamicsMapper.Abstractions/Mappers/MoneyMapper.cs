using Microsoft.Xrm.Sdk;

namespace DynamicsMapper.Abstractions.Mappers
{
    public static class MoneyMapper
    {
        public static Money? MapToEntity(decimal? value) => value.HasValue ? new Money(value.Value) : null;
        public static decimal? MapToModel(Entity entity, string attribute) => entity.GetAttributeValue<Money>(attribute)?.Value;
    }
}
