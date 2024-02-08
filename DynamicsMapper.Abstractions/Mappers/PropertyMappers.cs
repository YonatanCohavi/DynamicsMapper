using System;

namespace DynamicsMapper.Abstractions.Mappers
{
    public class PropertyMappers : IPropertyMappers
    {
        private readonly Lazy<IBasicMapper> _defaultBasicMaper = new(() => Mappers.BasicMapper.Instance);
        private readonly Lazy<IFormattedValuesMapper> _defaultFormattedValuesMapper = new(() => Mappers.FormattedValuesMapper.Instance);
        private readonly Lazy<IMoneyMapper> _defaultMoneyMapper = new(() => Mappers.MoneyMapper.Instance);
        private readonly Lazy<IOptionsetMapper> _defaultOptionsetMapper = new(() => Mappers.OptionsetMapper.Instance);
        private readonly Lazy<IOptionSetValueCollectionMapper> _defaultOptionSetValueCollectionMapper = new(() => Mappers.OptionSetValueCollectionMapper.Instance);
        private readonly Lazy<IPrimaryIdMapper> _defaultPrimaryIdMapper = new(() => Mappers.PrimaryIdMapper.Instance);
        private readonly Lazy<ILookupMapper> _defaultLookupMapper = new(() => Mappers.LookupMapper.Instance);
        private readonly Lazy<IDynamicLookupTargetMapper> _defaultDynamicLookupTargetMapper = new(() => Mappers.DynamicLookupTargetMapper.Instance);

        internal IBasicMapper? _basicMapper;
        internal IFormattedValuesMapper? _formattedValuesMapper;
        internal IMoneyMapper? _moneyMapper;
        internal IOptionsetMapper? _optionsetMapper;
        internal IOptionSetValueCollectionMapper? _optionSetValueCollectionMapper;
        internal IPrimaryIdMapper? _primaryIdMapper;
        internal ILookupMapper? _primaryLookupMapper;
        internal IDynamicLookupTargetMapper? _primaryDynamicLookupTargetMapper;

        public IBasicMapper BasicMapper
        {
            get => _basicMapper ?? _defaultBasicMaper.Value;
            set => _basicMapper = value;
        }
        public IFormattedValuesMapper FormattedValuesMapper
        {
            get => _formattedValuesMapper ?? _defaultFormattedValuesMapper.Value;
            set => _formattedValuesMapper = value;
        }
        public IMoneyMapper MoneyMapper
        {
            get => _moneyMapper ?? _defaultMoneyMapper.Value;
            set => _moneyMapper = value;
        }
        public IOptionsetMapper OptionsetMapper
        {
            get => _optionsetMapper ?? _defaultOptionsetMapper.Value;
            set => _optionsetMapper = value;
        }
        public IOptionSetValueCollectionMapper OptionSetValueCollectionMapper
        {
            get => _optionSetValueCollectionMapper ?? _defaultOptionSetValueCollectionMapper.Value;
            set => _optionSetValueCollectionMapper = value;
        }
        public IPrimaryIdMapper PrimaryIdMapper
        {
            get => _primaryIdMapper ?? _defaultPrimaryIdMapper.Value;
            set => _primaryIdMapper = value;
        }

        public ILookupMapper LookupMapper
        {
            get => _primaryLookupMapper ?? _defaultLookupMapper.Value;
            set => _primaryLookupMapper = value;
        }
        public IDynamicLookupTargetMapper DynamicLookupTargetMapper
        {
            get => _primaryDynamicLookupTargetMapper ?? _defaultDynamicLookupTargetMapper.Value;
            set => _primaryDynamicLookupTargetMapper = value;
        }
    }
}
