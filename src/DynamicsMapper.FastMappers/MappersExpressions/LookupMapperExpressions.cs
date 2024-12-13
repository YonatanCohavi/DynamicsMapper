using DynamicsMapper.Abstractions;
using DynamicsMapper.Abstractions.Mappers;
using System.Linq.Expressions;

namespace DynamicsMapper.FastMappers.MappersExpressions
{
    public static class LookupMapperExpressions
    {
        public static Expression ToModel(ParameterExpression settingsInput, ParameterExpression entityInput, string attributename, bool nullable)
        {
            var mappers = Expression.Property(settingsInput, nameof(DynamicsMapperSettings.Mappers));
            var mapper = Expression.Property(mappers, nameof(IPropertyMappers.LookupMapper));
            var call = Expression.Call(mapper, nameof(ILookupMapper.MapToModel), null, entityInput, Expression.Constant(attributename));
            if (nullable)
                return call;
            return Expression.Coalesce(call, Expression.Constant(Guid.Empty));
        }

        internal static Expression ToEntity(ParameterExpression settingsInput, MemberExpression propertyExpression, Expression targetExpression)
        {
            var mappers = Expression.Property(settingsInput, nameof(DynamicsMapperSettings.Mappers));
            var mapper = Expression.Property(mappers, nameof(IPropertyMappers.LookupMapper));
            return Expression.Call(mapper, nameof(ILookupMapper.MapToEntity), null, Expression.Convert(propertyExpression, typeof(Guid?)), targetExpression);
        }
    }
}
