using DynamicsMapper.Abstractions;
using DynamicsMapper.Abstractions.Mappers;
using System.Linq.Expressions;

namespace DynamicsMapper.FastMappers.MappersExpressions
{
    internal static class DynamicLookupTargetMappersExpressions
    {
        public static MethodCallExpression ToModel(ParameterExpression settingsInput, ParameterExpression entityInput, string attributename)
        {
            var mappers = Expression.Property(settingsInput, nameof(DynamicsMapperSettings.Mappers));
            var mapper = Expression.Property(mappers, nameof(IPropertyMappers.DynamicLookupTargetMapper));
            return Expression.Call(mapper, nameof(IDynamicLookupTargetMapper.MapToModel), null, entityInput, Expression.Constant(attributename));
        }
    }
}
