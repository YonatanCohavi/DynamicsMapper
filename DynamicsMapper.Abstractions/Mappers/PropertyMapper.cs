using System;

namespace DynamicsMapper.Abstractions.Mappers
{
    //public static class PropertyMapper
    //{
    //    private static IPropertyMapper<TModel, TEntity> GetMapper<TModel, TEntity>()
    //    {
    //        if (typeof(TModel) == typeof(TEntity))
    //            return (IPropertyMapper<TModel, TEntity>)new BasicMapper<TModel>();

    //        return typeof(TModel).Name switch
    //        {
    //            typeof(decimal?).Name,
    //            typeof(decimal).Name =>
    //            _ =>
    //            throw new PropertyMapperNoFoundException()
    //        };

    //    }
    //    public static TEntity MapToEntity<TModel, TEntity>(TModel value) => GetMapper<TModel, TEntity>().MapToEntity(value);
    //    public static TModel MapToModel<TModel, TEntity>(Entity entity, string attribute) => GetMapper<TModel, TEntity>().MapToModel(entity, attribute);
    //}
    internal class Test
    {
        private void E()
        {
            var m = PrimaryIdMapper.MapToModel(null, string.Empty);
            //var m2 = OptionSetValueCollectionMapper.MapToModel<cot>(null, string.Empty);
            var e = PrimaryIdMapper.MapToEntity(Guid.Empty);
        }
    }
}
