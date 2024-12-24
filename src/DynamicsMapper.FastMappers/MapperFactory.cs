using DynamicsMapper.Abstractions;
using DynamicsMapper.Abstractions.Settings;
using DynamicsMapper.FastMappers.MappersExpressions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicsMapper.FastMappers
{
    public static class MapperFactory
    {
        private class MembersMapping(PropertyInfo? source, PropertyInfo destination, Expression? overrideExpression)
        {
            public PropertyInfo? Source { get; } = source;
            public PropertyInfo Destination { get; } = destination;
            public Expression? OverrideExpression { get; } = overrideExpression;
        }

        private interface IBodyBuilder
        {
            void AddMember(MemberInfo destination, Expression mapping);
            IEnumerable<MembersMapping> GetMembers(Expression body);
            Expression Build<T>(Expression expression);
        }
        private class MemberInitBodyBuilder : IBodyBuilder
        {
            private readonly List<MemberBinding> _bindings = [];

            public void AddMember(MemberInfo destination, Expression mapping)
            {
                _bindings.Add(Expression.Bind(destination, mapping));
            }

            public Expression Build<T>(Expression expression)
            {
                if (expression is not MemberInitExpression memberinit)
                    throw new Exception("MemberInitExpression is expected");
                return memberinit.Update(Expression.New(typeof(T)), _bindings);
            }

            public IEnumerable<MembersMapping> GetMembers(Expression body)
            {
                if (body is not MemberInitExpression memberinit)
                    throw new Exception("MemberInitExpression is expected");

                return memberinit.Bindings.Select(b =>
                {
                    if (b is not MemberAssignment assingment)
                        throw new Exception("member assingment is expected");

                    MemberExpression? memberExpression = null;
                    var destinationPropInfo = (PropertyInfo)assingment.Member;

                    if (assingment.Expression is UnaryExpression unaryExpression)
                        memberExpression = (MemberExpression)unaryExpression.Operand;

                    if (assingment.Expression is MemberExpression sourceMemberExpression)
                        memberExpression = sourceMemberExpression;

                    if (assingment.Expression is ConstantExpression constantExpression)
                        return new MembersMapping(null, destinationPropInfo, constantExpression);

                    if (memberExpression != null)
                    {
                        var sourceMember = (PropertyInfo)memberExpression.Member;
                        var nullable = Nullable.GetUnderlyingType(memberExpression.Member.ReflectedType) != null;
                        if (nullable)
                            sourceMember = (PropertyInfo)((MemberExpression)memberExpression.Expression).Member;

                        return new MembersMapping(sourceMember, destinationPropInfo, null);
                    }
                    throw new Exception($"{assingment.Expression.NodeType} expresion is not supported");
                });
            }
        }
        private class NewExpressionBodyBuilder : IBodyBuilder
        {
            private readonly List<Expression> _mappings = [];
            public void AddMember(MemberInfo destination, Expression mapping) => _mappings.Add(mapping);

            public IEnumerable<MembersMapping> GetMembers(Expression body)
            {
                if (body is not NewExpression newExpression)
                    throw new Exception("NewExpression is expected");
                var zip = newExpression.Members.Zip(newExpression.Arguments, (m, a) => (member: m, argument: a));
                return zip.Select(z =>
                {
                    var (member, argument) = z;
                    var memberProp = (PropertyInfo)member;
                    if (argument is ConstantExpression constantExpression)
                        return new MembersMapping(null, memberProp, argument);

                    if (argument is MemberExpression memberExpression)
                    {
                        var sourceMember = (PropertyInfo)memberExpression.Member;
                        var nullable = Nullable.GetUnderlyingType(memberExpression.Member.ReflectedType) != null;
                        if (nullable)
                            sourceMember = (PropertyInfo)((MemberExpression)memberExpression.Expression).Member;

                        return new MembersMapping(sourceMember, memberProp, null);
                    }
                    if (argument is ConstantExpression constant)
                        return new MembersMapping(null, memberProp, argument);

                    throw new ArgumentException($"Unexpected argument: {argument.NodeType} ({argument.GetType()}");
                });
            }

            public Expression Build<T>(Expression expression)
            {
                if (expression is not NewExpression newExpression)
                    throw new Exception("NewExpression is expected");
                return newExpression.Update(_mappings);
            }
        }
        internal static FastModelMapper<TPModel> ToModelMapper<TModel, TPModel>(Expression<Func<TModel, TPModel>> selector)
        {
            var entityInput = Expression.Parameter(typeof(Entity), "entity");
            var settingsInput = Expression.Parameter(typeof(DynamicsMapperSettings), "settings");

            IBodyBuilder bodyBuilder = selector.Body switch
            {
                MemberInitExpression memberinit => new MemberInitBodyBuilder(),
                NewExpression newExpression => new NewExpressionBodyBuilder(),
                _ => throw new Exception($"{selector.Body.NodeType} is not supported. expecting NewExpression (new {{....}}) or MemberInitExpression (new ModelObject {{....}})"),
            };
            var members = bodyBuilder.GetMembers(selector.Body);
            if (!members.Any())
                throw new Exception("Unable to extract attributes. make sure that the passed model has attributes");

            var columns = new HashSet<string>();
            var sourceModel = typeof(TModel);
            var entityName = sourceModel.GetCustomAttribute<CrmEntityAttribute>()?.LogicalName;
            if (string.IsNullOrEmpty(entityName))
                throw new Exception($"Entity name is not defined for {typeof(TModel)}");

            var modelProperties = sourceModel.GetProperties();
            var newArguments = new List<Expression>();
            var memberAssingemnts = new List<MemberAssignment>();
            foreach (var member in bodyBuilder.GetMembers(selector.Body))
            {
                if (member.OverrideExpression != null)
                {
                    bodyBuilder.AddMember(member.Destination, member.OverrideExpression);
                    continue;
                }

                if (member.Source == null)
                    throw new Exception("soruce property info is null");

                var property = modelProperties.SingleOrDefault(p => p.Name == member.Source.Name)
                    ?? throw new Exception($"{member.Source.Name} not found on {sourceModel.FullName}");

                var crmField = property.GetCustomAttribute<CrmFieldAttribute>();
                if (crmField == null)
                    continue;

                columns.Add(crmField.SchemaName);
                var mapping = GetMapping(member.Destination.PropertyType, crmField, settingsInput, entityInput);
                bodyBuilder.AddMember(member.Destination, mapping);
            }

            var body = bodyBuilder.Build<TPModel>(selector.Body);
            var lamda = Expression.Lambda<Func<Entity, DynamicsMapperSettings, TPModel>>(body, entityInput, settingsInput);
            var compiled = lamda.Compile();
            return new FastModelMapper<TPModel>(new ColumnSet([.. columns]), (e, settings) => compiled(e, settings), entityName!);
        }

        private static Expression GetMapping(Type destinationType, CrmFieldAttribute crmField, ParameterExpression settingsInput, ParameterExpression entityInput)
        {
            var schemaName = crmField.SchemaName;
            var nullable = Nullable.GetUnderlyingType(destinationType) != null;
            return crmField.Mapping switch
            {
                MappingType.Basic or 0 => BasicMapperExpressions.ToModel(settingsInput, entityInput, schemaName, destinationType),
                MappingType.Lookup => LookupMapperExpressions.ToModel(settingsInput, entityInput, schemaName, nullable),
                MappingType.Money => MoneyMapperExpressions.ToModel(settingsInput, entityInput, schemaName, nullable),
                MappingType.Formatted => FormattedValuesMapperExpressions.ToModel(settingsInput, entityInput, schemaName),
                MappingType.Options => OptionsMapperExpressions.ToModel(settingsInput, entityInput, schemaName, destinationType),
                MappingType.MultipleOptions => MultipleOptionsMappersExpressions.ToModel(settingsInput, entityInput, schemaName, destinationType),
                MappingType.PrimaryId => PrimaryIdMapperExpressions.ToModel(settingsInput, entityInput, schemaName, nullable),
                MappingType.DynamicLookup => LookupMapperExpressions.ToModel(settingsInput, entityInput, schemaName, nullable),
                MappingType.DynamicLookupTarget => DynamicLookupTargetMappersExpressions.ToModel(settingsInput, entityInput, schemaName),
                MappingType.Link => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
        }

        internal static FastEntityMapper<TPModel> ToEntityMapper<TModel, TPModel>(Expression<Func<TModel, TPModel>> selector)
        {
            IBodyBuilder bodyBuilder = selector.Body switch
            {
                MemberInitExpression memberinit => new MemberInitBodyBuilder(),
                NewExpression newExpression => new NewExpressionBodyBuilder(),
                _ => throw new Exception($"{selector.Body.NodeType} is not supported. expecting NewExpression (new {{....}}) or MemberInitExpression (new ModelObject {{....}})"),
            };

            var columns = new HashSet<string>();
            var sourceModel = typeof(TModel);
            var entityName = sourceModel.GetCustomAttribute<CrmEntityAttribute>()?.LogicalName;
            if (string.IsNullOrEmpty(entityName))
                throw new Exception("entity name is not defined");

            var modelProperties = sourceModel.GetProperties();

            var modelInput = Expression.Parameter(typeof(TPModel), "model");
            var settingsInput = Expression.Parameter(typeof(DynamicsMapperSettings), "settings");

            var entity = Expression.Variable(typeof(Entity), "entity");
            var newEntity = Expression.New(typeof(Entity));

            var newArguments = new List<Expression>() {
                Expression.Assign(entity, newEntity),
                Expression.Assign(Expression.Property(entity, nameof(Entity.LogicalName)), Expression.Constant(entityName)),
            };

            var attributes = Expression.Property(entity, nameof(Entity.Attributes));
            var keepDefaultValues =
                   Expression.NotEqual(Expression.Constant(DefaultValueHandling.Ignore),
                   Expression.Property(settingsInput, nameof(DynamicsMapperSettings.DefaultValueHandling)));

            var members = bodyBuilder.GetMembers(selector.Body);
            var schemaNames = new HashSet<string>();
            foreach (var member in members)
            {
                if (member.OverrideExpression != null)
                {
                    bodyBuilder.AddMember(member.Destination, member.OverrideExpression);
                    continue;
                }
                if (member.Source == null)
                    throw new Exception("soruce property info is null");
                var property = modelProperties.SingleOrDefault(p => p.Name == member.Source.Name)
                    ?? throw new Exception($"{member.Source.Name} not found on {sourceModel.FullName}");

                var crmField = property.GetCustomAttribute<CrmFieldAttribute>();
                if (crmField == null)
                    continue;
                var schemaName = crmField.SchemaName;
                Expression targetExpression = Expression.Constant(crmField.Target);
                var propertyExpression = Expression.Property(modelInput, member.Destination.Name);
                if (crmField.Mapping == MappingType.Formatted)
                    continue;
                if (crmField.Mapping == MappingType.DynamicLookup)
                {
                    var targetProperty = modelProperties.SingleOrDefault(p =>
                    {
                        var att = p.GetCustomAttribute<CrmFieldAttribute>();
                        if (att == null)
                            return false;
                        if (att.Mapping != MappingType.DynamicLookupTarget)
                            return false;
                        if (att.SchemaName != schemaName)
                            return false;
                        return true;
                    }) ?? throw new Exception($"DynamicLookupTarget not found for {schemaName}");
                    targetExpression = Expression.Property(modelInput, targetProperty.Name);
                }
                if (crmField.Mapping == MappingType.DynamicLookupTarget)
                    continue;

                Expression value = crmField.Mapping switch
                {
                    MappingType.Basic or 0 => BasicMapperExpressions.ToEntity(settingsInput, propertyExpression, member.Destination.PropertyType),
                    MappingType.Lookup => LookupMapperExpressions.ToEntity(settingsInput, propertyExpression, targetExpression),
                    MappingType.Money => MoneyMapperExpressions.ToEntity(settingsInput, propertyExpression),
                    MappingType.Options => OptionsMapperExpressions.ToEntity(settingsInput, propertyExpression, member.Destination.PropertyType),
                    MappingType.MultipleOptions => MultipleOptionsMappersExpressions.ToEntity(settingsInput, propertyExpression, member.Destination.PropertyType),
                    MappingType.PrimaryId => PrimaryIdMapperExpressions.ToEntity(settingsInput, propertyExpression),
                    MappingType.DynamicLookup => LookupMapperExpressions.ToEntity(settingsInput, propertyExpression, targetExpression),
                    MappingType.DynamicLookupTarget => throw new NotImplementedException(),
                    MappingType.Formatted => throw new NotImplementedException(),
                    MappingType.Link => throw new NotImplementedException(),
                    _ => throw new NotImplementedException(),
                };
                if (!schemaNames.Add(schemaName))
                    throw new Exception($"{schemaName} is defined twice on {typeof(TPModel)}");

                var mapping = Expression.Call(attributes, nameof(Entity.Attributes.Add), null, Expression.Constant(schemaName), Expression.Convert(value, typeof(object)));
                if (crmField.Mapping == MappingType.PrimaryId)
                {
                    var assingId = Expression.Assign(Expression.Property(entity, "Id"), Expression.Convert(value, typeof(Guid)));
                    newArguments.Add(assingId);
                }
                var def = Expression.IfThen(Expression.Or(keepDefaultValues, Expression.NotEqual(propertyExpression, Expression.Default(member.Destination.PropertyType))), mapping);
                newArguments.Add(def);
            }

            newArguments.Add(entity);
            var body = Expression.Block([entity], newArguments);

            var lamda = Expression.Lambda<Func<TPModel, DynamicsMapperSettings, Entity>>(body, modelInput, settingsInput);
            var compiled = lamda.Compile();
            return new FastEntityMapper<TPModel>(compiled);
        }
    }
}
;