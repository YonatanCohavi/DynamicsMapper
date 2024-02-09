using DynamicsMapper.Abstractions.Mappers;
using DynamicsMapper.Abstractions.Settings;

namespace DynamicsMapper.Abstractions
{
    public class DynamicsMapperSettings
    {
        private const NullHandling _defaultNullHandling = NullHandling.Map;
        private NullHandling? _nullHandling;

        public static DynamicsMapperSettings Default => new();
        public IPropertyMappers Mappers { get; set; } = new PropertyMappers();
        public NullHandling NullHandling
        {
            get => _nullHandling ?? _defaultNullHandling;
            set => _nullHandling = value;
        }
    }
}
