using DynamicsMapper.Abstractions;
using DynamicsMapper.Abstractions.Mappers;
using System.Linq.Expressions;

namespace DynamicsMapper.FastMappers.MappersExpressions
{
    internal static class MultipleOptionsMappersExpressions
    {
        internal static MethodCallExpression ToModel(ParameterExpression settingsInput, ParameterExpression entityInput, string attributename, Type desitinationType)
        {
            var mappers = Expression.Property(settingsInput, nameof(DynamicsMapperSettings.Mappers));
            var mapper = Expression.Property(mappers, nameof(IPropertyMappers.OptionSetValueCollectionMapper));
            if (desitinationType.IsEnum)
                return Expression.Call(mapper, nameof(IOptionSetValueCollectionMapper.MapToModel), [desitinationType], entityInput, Expression.Constant(attributename));
            return Expression.Call(mapper, nameof(IOptionSetValueCollectionMapper.MapToModel), null, entityInput, Expression.Constant(attributename));
        }
        internal static Expression ToEntity(ParameterExpression settingsInput, MemberExpression propertyExpression, Type sourceType)
        {
            var mappers = Expression.Property(settingsInput, nameof(DynamicsMapperSettings.Mappers));
            var mapper = Expression.Property(mappers, nameof(IPropertyMappers.OptionSetValueCollectionMapper));
            var itemType = sourceType.GetElementType()
                ?? throw new Exception("there is no item type");

            if (itemType.IsEnum)
                return Expression.Call(mapper, nameof(IOptionSetValueCollectionMapper.MapToEntity), [itemType], propertyExpression);

            return Expression.Call(mapper, nameof(IOptionSetValueCollectionMapper.MapToEntity), null, propertyExpression);
        }
    }
}
