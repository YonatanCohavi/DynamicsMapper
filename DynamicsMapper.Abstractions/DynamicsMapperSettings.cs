using DynamicsMapper.Abstractions.Mappers;
using DynamicsMapper.Abstractions.Settings;

namespace DynamicsMapper.Abstractions
{
    public class DynamicsMapperSettings
    {
        public static DynamicsMapperSettings Default => new();

        private const DefaultValueHandling _defaultDefaultValueHandling = DefaultValueHandling.Map;
        private DefaultValueHandling? _defaultValueHandling;
        public IPropertyMappers Mappers { get; set; } = new PropertyMappers();
        public DefaultValueHandling DefaultValueHandling
        {
            get => _defaultValueHandling ?? _defaultDefaultValueHandling;
            set => _defaultValueHandling = value;
        }
    }
}
