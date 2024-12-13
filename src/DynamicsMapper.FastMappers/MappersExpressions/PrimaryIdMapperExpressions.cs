using DynamicsMapper.Abstractions;
using DynamicsMapper.Abstractions.Mappers;
using System.Linq.Expressions;

namespace DynamicsMapper.FastMappers.MappersExpressions
{
    internal static class PrimaryIdMapperExpressions
    {

        internal static Expression ToModel(ParameterExpression settingsInput, ParameterExpression entityInput, string attributename, bool nullable)
        {
            var mappers = Expression.Property(settingsInput, nameof(DynamicsMapperSettings.Mappers));
            var mapper = Expression.Property(mappers, nameof(IPropertyMappers.PrimaryIdMapper));
            var call = Expression.Call(mapper, nameof(IPrimaryIdMapper.MapToModel), null, entityInput, Expression.Constant(attributename));
            if (nullable)
                return call;
            return Expression.Coalesce(call, Expression.Constant(Guid.Empty));
        }
        internal static Expression ToEntity(ParameterExpression settingsInput, MemberExpression propertyExpression)
        {
            var mappers = Expression.Property(settingsInput, nameof(DynamicsMapperSettings.Mappers));
            var mapper = Expression.Property(mappers, nameof(IPropertyMappers.PrimaryIdMapper));
            return Expression.Call(mapper, nameof(IPrimaryIdMapper.MapToEntity), null, Expression.Convert(propertyExpression, typeof(Guid?)));
        }
    }
}
