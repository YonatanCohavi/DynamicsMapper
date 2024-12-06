using Microsoft.Xrm.Sdk;

namespace DynamicsMapper.Abstractions.Mappers
{
    public class MoneyMapper : IMoneyMapper
    {
        private static MoneyMapper? _mapper;
        public static MoneyMapper Instance => _mapper ??= new MoneyMapper();
        private MoneyMapper() { }
        public Money? MapToEntity(decimal? value) => value.HasValue ? new Money(value.Value) : null;
        public decimal? MapToModel(Entity entity, string attribute) => entity.GetAttributeValue<Money>(attribute)?.Value;
    }

    public interface IMoneyMapper
    {
        Money? MapToEntity(decimal? value);
        decimal? MapToModel(Entity entity, string attribute);
    }
}
