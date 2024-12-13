using DynamicsMapper.Abstractions;
using Microsoft.Xrm.Sdk;

namespace DynamicsMapper.FastMappers
{
    public class FastEntityMapper<T>(Func<T, DynamicsMapperSettings, Entity> mappingFunc)
    {
        private readonly Func<T, DynamicsMapperSettings, Entity> _mappingFunc = mappingFunc;
        public Entity Map(T model) => _mappingFunc(model, DynamicsMapperSettings.Default);
        public Entity Map(T model, DynamicsMapperSettings settings) => _mappingFunc(model, settings);
    }
}
;