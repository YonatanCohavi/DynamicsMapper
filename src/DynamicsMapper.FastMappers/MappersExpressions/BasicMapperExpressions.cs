using DynamicsMapper.Abstractions;
using DynamicsMapper.Abstractions.Mappers;
using System.Linq.Expressions;

namespace DynamicsMapper.FastMappers.MappersExpressions
{
    public static class BasicMapperExpressions
    {
        public static Expression ToModel(ParameterExpression settingsInput, ParameterExpression entityInput, string attributename, Type propertyType)
        {
            var mappers = Expression.Property(settingsInput, nameof(DynamicsMapperSettings.Mappers));
            var mapper = Expression.Property(mappers, nameof(IPropertyMappers.BasicMapper));
            var nullable = Nullable.GetUnderlyingType(propertyType) != null;
            return Expression.Call(mapper, nameof(IBasicMapper.MapToModel), [propertyType], entityInput, Expression.Constant(attributename));
        }

        internal static Expression ToEntity(ParameterExpression settingsInput, MemberExpression propertyExpression, Type propertyType)
        {
            var mappers = Expression.Property(settingsInput, nameof(DynamicsMapperSettings.Mappers));
            var mapper = Expression.Property(mappers, nameof(IPropertyMappers.BasicMapper));
            return Expression.Call(mapper, nameof(IBasicMapper.MapToEntity), [propertyType], propertyExpression);
        }
    }
}
