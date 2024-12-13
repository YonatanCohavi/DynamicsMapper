using DynamicsMapper.Abstractions;
using DynamicsMapper.Abstractions.Mappers;
using System.Linq.Expressions;

namespace DynamicsMapper.FastMappers.MappersExpressions
{
    public static class FormattedValuesMapperExpressions
    {
        public static MethodCallExpression ToModel(ParameterExpression settingsInput, ParameterExpression entityInput, string attributename)
        {
            var mappers = Expression.Property(settingsInput, nameof(DynamicsMapperSettings.Mappers));
            var mapper = Expression.Property(mappers, nameof(IPropertyMappers.FormattedValuesMapper));
            return Expression.Call(mapper, nameof(IFormattedValuesMapper.MapToModel), null, entityInput, Expression.Constant(attributename));
        }
    }
}
