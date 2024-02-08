namespace DynamicsMapper.Abstractions.Mappers
{
    public interface IPropertyMappers
    {
        IBasicMapper BasicMapper { get; set; }
        IFormattedValuesMapper FormattedValuesMapper { get; set; }
        IMoneyMapper MoneyMapper { get; set; }
        IOptionsetMapper OptionsetMapper { get; set; }
        IOptionSetValueCollectionMapper OptionSetValueCollectionMapper { get; set; }
        IPrimaryIdMapper PrimaryIdMapper { get; set; }
        ILookupMapper LookupMapper { get; set; }
        IDynamicLookupTargetMapper DynamicLookupTargetMapper { get; set; }
    }
}