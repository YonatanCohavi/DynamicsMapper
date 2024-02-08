using DynamicsMapper.Abstractions.Mappers;

namespace DynamicsMapper.Abstractions
{
    public class DynamicsMapperSettings
    {
        public static DynamicsMapperSettings Default => new();
        public IPropertyMappers Mappers { get; set; } = new PropertyMappers();
    }
}
