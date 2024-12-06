using Microsoft.Xrm.Sdk;

namespace DynamicsMapper.Abstractions.Mappers
{
    public class BasicMapper : IBasicMapper
    {
        private static BasicMapper? _mapper;
        public static BasicMapper Instance => _mapper ??= new BasicMapper();
        public T MapToEntity<T>(T value) => value;
        public T MapToModel<T>(Entity entity, string attribute) => entity.GetAttributeValue<T>(attribute);
    }

    public interface IBasicMapper
    {
        T MapToEntity<T>(T value);
        T MapToModel<T>(Entity entity, string attribute);
    }
}
