using Microsoft.Xrm.Sdk;
using System;

namespace DynamicsMapper.Abstractions.Mappers
{
    public interface IPropertyMapper<TModel, TEntity>
    {
        TEntity MapToEntity(TModel value);
        TModel MapToModel(Entity entity, string attribute);
    }
    public class BasicMapper<T> : IPropertyMapper<T, T>
    {
        public T MapToEntity(T value) => value;
        public T MapToModel(Entity entity, string attribute) => entity.GetAttributeValue<T>(attribute);
    }
    public static class PropertyMapper
    {
        private static IPropertyMapper<TModel, TEntity> GetMapper<TModel, TEntity>()
        {
            if (typeof(TModel) == typeof(TEntity))
                return (IPropertyMapper<TModel, TEntity>)new BasicMapper<TModel>();

            return typeof(TModel) switch
            {
                _ =>
                throw new PropertyMapperNoFoundException()
            };

        }
        public static TEntity MapToEntity<TModel, TEntity>(TModel value) => GetMapper<TModel, TEntity>().MapToEntity(value);
        public static TModel MapToModel<TModel, TEntity>(Entity entity, string attribute) => GetMapper<TModel, TEntity>().MapToModel(entity, attribute);
    }
    internal class Test
    {
        private void E()
        {
            var a = PropertyMapper.MapToModel<DateTime?, DateTime?>(null, string.Empty);
            var a1 = PropertyMapper.MapToEntity<DateTime?, DateTime?>(DateTime.Now);
        }
    }
}
