using System;

namespace DynamicsMapper.Abstractions.Mappers
{
    public static class PropertyMappers
    {
        public static Func<IBasicMapper> GetBasicMapper { get; set; } = () => BasicMapper.Instance;
        public static Func<IFormattedValuesMapper> GetFormattedValuesMapper { get; set; } = () => FormattedValuesMapper.Instance;
        public static Func<ILookupMapper> GetLookupMapper { get; set; } = () => LookupMapper.Instance;
        public static Func<IMoneyMapper> GetMoneyMapper { get; set; } = () => MoneyMapper.Instance;
        public static Func<IOptionsetMapper> GetOptionsetMapper { get; set; } = () => OptionsetMapper.Instance;
        public static Func<IOptionSetValueCollectionMapper> GetOptionSetValueCollectionMapper { get; set; } = () => OptionSetValueCollectionMapper.Instance;
        public static Func<IPrimaryIdMapper> GetPrimaryIdMapper { get; set; } = () => PrimaryIdMapper.Instance;
    }
}
