using DynamicsMapper.Abstractions;
using DynamicsMapper.Abstractions.Mappers;
using System.Linq.Expressions;

namespace DynamicsMapper.FastMappers.MappersExpressions
{
    internal static class OptionsMapperExpressions
    {

        internal static Expression ToModel(ParameterExpression settingsInput, ParameterExpression entityInput, string schemaName, Type destinationType)
        {
            var mappers = Expression.Property(settingsInput, nameof(DynamicsMapperSettings.Mappers));
            var mapper = Expression.Property(mappers, nameof(IPropertyMappers.OptionsetMapper));
            var nullable = Nullable.GetUnderlyingType(destinationType) != null;
            if (nullable)
            {
                var call = Expression.Call(mapper, nameof(IOptionsetMapper.MapToModel), null, entityInput, Expression.Constant(schemaName));
                if (destinationType == typeof(int?))
                    return call;
                return Expression.Convert(call, destinationType);
            }
            else
            {
                var call = Expression.Call(mapper, nameof(IOptionsetMapper.MapToModel), null, entityInput, Expression.Constant(schemaName));
                if (destinationType == typeof(int))
                    return Expression.Coalesce(call, Expression.Default(typeof(int)));
                return Expression.Coalesce(Expression.Convert(call, destinationType), Expression.Default(destinationType));
            }
        }

        internal static Expression ToEntity(ParameterExpression settingsInput, MemberExpression propertyExpression, Type sourceType)
        {
            var mappers = Expression.Property(settingsInput, nameof(DynamicsMapperSettings.Mappers));
            var mapper = Expression.Property(mappers, nameof(IPropertyMappers.OptionsetMapper));
            var source = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
            return Expression.Call(mapper, nameof(IOptionsetMapper.MapToEntity), null, Expression.Convert(propertyExpression, typeof(int?)));
        }

    }
}
