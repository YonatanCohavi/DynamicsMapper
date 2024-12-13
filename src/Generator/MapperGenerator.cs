using DynamicsMapper.Abstractions;
using DynamicsMapper.Extentions;
using DynamicsMapper.Helpers;
using DynamicsMapper.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DynamicsMapper
{
    [Generator]
    public class MapperGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classes = context.SyntaxProvider
                .ForAttributeWithMetadataName(typeof(CrmEntityAttribute).FullName,
                    static (s, _) => s is ClassDeclarationSyntax,
                    static (ctx, _) => ctx.TargetNode as ClassDeclarationSyntax)
                .Where(c => c is not null);

            var compilationAndMappers = context.CompilationProvider.Combine(classes.Collect());
            context.RegisterImplementationSourceOutput(compilationAndMappers,
                static (spc, source) => Execute(source.Left, source.Right!, spc));
        }

        private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> mappers, SourceProductionContext ctx)
        {
            if (mappers.IsDefaultOrEmpty)
                return;
            GenerateEntityExtentionClass(ctx);
            var crmLinkAttributeSymbol = compilation.GetTypeByMetadataName(typeof(CrmLinkAttribute).FullName);
            var crmEntityAttributeSymbol = compilation.GetTypeByMetadataName(typeof(CrmEntityAttribute).FullName);
            var crmFieldAttributeSymbol = compilation.GetTypeByMetadataName(typeof(CrmFieldAttribute).FullName);

            if (crmFieldAttributeSymbol == null)
                return;
            if (crmLinkAttributeSymbol == null)
                return;


            if (crmEntityAttributeSymbol == null)
                return;
            var createdMappers = new List<MapperDetails>();
            foreach (var mapperSyntax in mappers.Distinct())
            {
                var mapperModel = compilation.GetSemanticModel(mapperSyntax.SyntaxTree);
                if (mapperModel.GetDeclaredSymbol(mapperSyntax) is not INamedTypeSymbol mapperSymbol)
                    continue;

                var crmAttributeData = mapperSymbol.GetAttribute(crmEntityAttributeSymbol).FirstOrDefault();
                if (crmAttributeData is null)
                    continue;
                var a = mapperSyntax.Identifier.ValueText;
                var entityName = (string)crmAttributeData.ConstructorArguments[0].Value!;
                var mapperName = crmAttributeData.NamedArguments.FirstOrDefault(na => na.Key == nameof(CrmEntityAttribute.MapperName));
                var mapperClassName = mapperName.Value.Value as string ?? $"{mapperSyntax.Identifier.ValueText}Mapper";
                createdMappers.Add(new MapperDetails(mapperSyntax, mapperSymbol, mapperClassName, entityName));
            }
            foreach (var mapperSyntax in mappers.Distinct())
            {
                var details = createdMappers.Single(m => m.ClassDeclarationSyntax == mapperSyntax);
                var properties = details.MapperSymbol.GetAllProperties();

                var fieldsGenerationDetails = ExtractAttributes(properties, crmFieldAttributeSymbol, crmLinkAttributeSymbol, ctx)
                    .ToArray();

                var duplicateSchemas = fieldsGenerationDetails
                    .Where(dg => dg.Mapping != MappingType.DynamicLookupTarget && dg.Mapping != MappingType.Formatted && !string.IsNullOrEmpty(dg.SchemaName))
                    .GroupBy(dg => dg.SchemaName!)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g);

                var duplicateMappings = fieldsGenerationDetails
                  .Where(dg => !string.IsNullOrEmpty(dg.SchemaName))
                  .GroupBy(dg => new { dg.Mapping, dg.SchemaName })
                  .Where(g => g.Count() > 1)
                  .SelectMany(g => g);

                foreach (var duplicate in duplicateSchemas)
                    duplicate.PropertySymbol.SetDiagnostic(ctx, DiagnosticsDescriptors.DuplicateSchemas, details.MapperSymbol.Name, duplicate.SchemaName!);

                foreach (var duplicate in duplicateMappings)
                    duplicate.PropertySymbol.SetDiagnostic(ctx, DiagnosticsDescriptors.DuplicateMappings, details.MapperSymbol.Name, duplicate.SchemaName!, duplicate.Mapping.ToString());

                if (fieldsGenerationDetails.Count(gd => gd.Mapping == MappingType.PrimaryId) > 1)
                {
                    details.MapperSymbol.SetDiagnostic(ctx, DiagnosticsDescriptors.MultiplePrimaryIds, details.MapperSymbol.Name);
                    continue;
                }

                var mapperClass = GenerateMapperClass(mapperSyntax, details.EntityName, fieldsGenerationDetails, details.MapperClassName, createdMappers, ctx);
                // verify compilation
                mapperClass.Code = SyntaxFactory
                      .ParseCompilationUnit(mapperClass.Code)
                      .NormalizeWhitespace()
                      .GetText()
                      .ToString();
                ctx.AddSource($"{mapperClass.ClassName}.g.cs", mapperClass.Code);
            }
        }

        private static void GenerateEntityExtentionClass(SourceProductionContext ctx)
        {
            var writer = new CodeWriter();
            writer.AppendLine("// <auto-generated />");
            writer.AppendLine("#nullable enable");
            writer.AppendLine();
            writer.AddUsing("Microsoft.Xrm.Sdk");
            writer.AddUsing("System.Linq");
            writer.AppendLine();
            using (writer.BeginScope($"namespace DynamicsMapper.Extension"))
            {
                using (writer.BeginScope($"public static class EntityExtension"))
                {
                    using (writer.BeginScope($"public static Entity? GetAliasedEntity(this Entity entity, string alias)"))
                    {
                        writer.AppendLine("var attributes = entity.Attributes.Where(e => e.Key.StartsWith(alias)).ToArray();");
                        writer.AppendLine("if (!attributes.Any()) return null;");
                        writer.AppendLine("var aliasEntity = new Entity();");
                        using (writer.BeginScope($"foreach (var attribute in attributes)"))
                        {
                            writer.AppendLine("if (!(attribute.Value is AliasedValue aliasedValued)) continue;");
                            writer.AppendLine("if (string.IsNullOrEmpty(aliasEntity.LogicalName))");
                            writer.AppendLine("aliasEntity.LogicalName = aliasedValued.EntityLogicalName;");
                            writer.AppendLine("aliasEntity[aliasedValued.AttributeLogicalName] = aliasedValued.Value;");
                        }
                        writer.AppendLine("return aliasEntity;");
                    }
                }

            }
            var classContent = SyntaxFactory
                                 .ParseCompilationUnit(writer.ToString())
                                 .NormalizeWhitespace()
                                 .GetText()
                                 .ToString();
            ctx.AddSource($"EntityExtentions.g.cs", classContent);
        }
        private static MapperClassDetails GenerateMapperClass(ClassDeclarationSyntax mapperSyntax, string entityName, FieldGenerationDetails[] attributes, string mapperClassName, ICollection<MapperDetails> createdMappers, SourceProductionContext ctx)
        {
            var columns = attributes.Where(a => !string.IsNullOrEmpty(a.SchemaName)).Select(a => $"\"{a.SchemaName}\"").Distinct().ToList();
            var @namespace = mapperSyntax.GetParent<BaseNamespaceDeclarationSyntax>()!.Name.ToString();
            var toEntityContent = new StringBuilder();
            var toModelContent = new StringBuilder();
            var className = mapperSyntax.Identifier.ValueText;
            var modelName = $"{char.ToLower(className[0])}{className.Substring(1)}";

            var mappings = new List<Mappings>();
            // dynamic lookups are using lookuptargets to resolve. we will map that last
            foreach (var attribute in attributes.Where(m => m.Mapping != MappingType.DynamicLookup))
            {
                var attMappings = GetMapperContent(modelName, attribute, createdMappers, ctx);
                var hasSetter = attribute.PropertySymbol.SetMethod is not null;

                if (!attMappings.HasValue)
                    continue;

                if (!string.IsNullOrEmpty(attMappings.Value.ToEntity))
                {
                    toEntityContent.AppendLine($"if(settings.DefaultValueHandling != DefaultValueHandling.Ignore || {attMappings.Value.AttributRef} != default({attMappings.Value.AttributeType}))");
                    toEntityContent.AppendLine("{");
                    toEntityContent.AppendLine(attMappings.Value.ToEntity);
                    toEntityContent.AppendLine("}");
                }

                if (hasSetter)
                {
                    toModelContent.AppendLine(attMappings.Value.ToModel);
                }
                mappings.Add(attMappings.Value);
            }
            foreach (var attribute in attributes.Where(m => m.Mapping == MappingType.DynamicLookup))
            {
                var targetlookup = mappings.FirstOrDefault(m => m.Type == MappingType.DynamicLookupTarget && m.Attribute.SchemaName == attribute.SchemaName);
                var attMappings = GenerateDynamicLookupMappings(modelName, targetlookup.AttributRef, attribute, ctx);
                var hasSetter = attribute.PropertySymbol.SetMethod is not null;

                if (!attMappings.HasValue)
                    continue;

                if (!string.IsNullOrEmpty(attMappings.Value.ToEntity))
                {
                    toEntityContent.AppendLine($"if(settings.DefaultValueHandling != DefaultValueHandling.Ignore || {attMappings.Value.AttributRef} != default({attMappings.Value.AttributeType}))");
                    toEntityContent.AppendLine("{");
                    toEntityContent.AppendLine(attMappings.Value.ToEntity);
                    toEntityContent.AppendLine("}");
                }

                if (hasSetter)
                {
                    toModelContent.AppendLine(attMappings.Value.ToModel);
                }
                mappings.Add(attMappings.Value);
            }
            var targetClassName = string.Empty;
            var hasDynamicLookups = mappings.Any(m => m.Type == MappingType.DynamicLookup);
            var dynamicMappingsClass = hasDynamicLookups
                ? GenerateDynamicsmappingClass(className, attributes, out targetClassName)
                : string.Empty;

            var writer = new CodeWriter();
            writer.AppendLine("// <auto-generated />");
            writer.AppendLine("#nullable enable");
            writer.AppendLine();
            writer.AddUsing("Microsoft.Xrm.Sdk");
            writer.AddUsing("Microsoft.Xrm.Sdk.Query");
            writer.AddUsing("DynamicsMapper.Extension");
            writer.AddUsing("DynamicsMapper.Abstractions.Settings");
            writer.AddUsing("DynamicsMapper.Abstractions");
            writer.AddUsing("System.Linq.Expressions");
            writer.AddUsing("DynamicsMapper.FastMappers");
            //writer.AddUsing("DynamicsMapper.Mappers");
            writer.AddUsing("System");
            writer.AppendLine();

            using (writer.BeginScope($"namespace {@namespace}"))
            {
                if (dynamicMappingsClass != string.Empty)
                    writer.AppendLine(dynamicMappingsClass);

                using (writer.BeginScope($"public class {mapperClassName} : IEntityMapper<{className}>"))
                {
                    writer.AppendLine($"private static readonly string[] columns = new[] {{{string.Join(", ", columns)}}};");
                    writer.AppendLine($"public ColumnSet Columns => new ColumnSet(columns);");
                    writer.AppendLine($"private const string entityname = \"{entityName}\";");
                    writer.AppendLine($"public string Entityname => entityname;");
                    writer.AppendLine($"public static FastMapper<{className}, TPModel> CreatePartialMapper<TPModel>(Expression<Func<{className}, TPModel>> selector) => new(selector);\n");

                    writer.AppendLine();


                    string dynamicTargetsSummary;
                    string hasDynamicLookupsWarning;
                    if (hasDynamicLookups)
                    {
                        dynamicTargetsSummary = $"/// <summary> You can use <see cref=\"{targetClassName}\"/> instead of <see cref=\"DynamicsMappingsTargets\"/></summary>";
                        hasDynamicLookupsWarning = $"/// <summary> <strong>NOTE: </strong>: <see cref=\"{className}\"/> requires the usage of <see cref=\"{targetClassName}\"/> or <see cref=\"DynamicsMappingsTargets\"/> if you are using this method then the targets should be passed using the '{nameof(MappingType.DynamicLookupTarget)}' mapping</summary>";
                    }
                    else
                    {
                        dynamicTargetsSummary = string.Empty;
                        hasDynamicLookupsWarning = string.Empty;
                    }
                    writer.AppendLine(dynamicTargetsSummary);
                    writer.AppendLine($"public Entity Map({className} {modelName}, DynamicsMappingsTargets dynamicMappingsTargets, DynamicsMapperSettings settings) => InternalMap({modelName}, dynamicMappingsTargets, settings);");
                    writer.AppendLine(dynamicTargetsSummary);
                    writer.AppendLine($"public Entity Map({className} {modelName}, DynamicsMappingsTargets dynamicMappingsTargets) => InternalMap({modelName}, dynamicMappingsTargets: dynamicMappingsTargets);");

                    writer.AppendLine(hasDynamicLookupsWarning);
                    writer.AppendLine($"public Entity Map({className} {modelName}, DynamicsMapperSettings settings) => InternalMap({modelName}, settings: settings);");
                    writer.AppendLine(hasDynamicLookupsWarning);
                    writer.AppendLine($"public Entity Map({className} {modelName}) => InternalMap({modelName});");

                    using (writer.BeginScope($"private static Entity InternalMap({className} {modelName}, DynamicsMappingsTargets? dynamicMappingsTargets = null, DynamicsMapperSettings? settings = null)"))
                    {
                        writer.AppendLine("settings ??= DynamicsMapperSettings.Default;");
                        writer.AppendLine("var mappers = settings.Mappers;");

                        writer.AppendLine($"var entity = new Entity(entityname);");
                        writer.AppendLine(toEntityContent.ToString());
                        writer.AppendLine("return entity;");
                    }

                    writer.AppendLine($"public {className}? Map(Entity entity, string alias) => InternalMap(entity, alias: alias);");
                    writer.AppendLine($"public {className}? Map(Entity entity, string alias, DynamicsMapperSettings settings) => InternalMap(entity, settings, alias);");

                    using (writer.BeginScope($"public {className} Map(Entity entity)"))
                    {
                        writer.AppendLine($"var {modelName} = InternalMap(entity) ?? throw new Exception(\"Mapping failed\");");
                        writer.AppendLine($"return {modelName};");
                    }
                    using (writer.BeginScope($"public {className} Map(Entity entity, DynamicsMapperSettings settings)"))
                    {
                        writer.AppendLine($"var {modelName} = InternalMap(entity, settings) ?? throw new Exception(\"Mapping failed\");");
                        writer.AppendLine($"return {modelName};");
                    }

                    using (writer.BeginScope($"private static {className}? InternalMap(Entity source, DynamicsMapperSettings? settings = null, string? alias = null)"))
                    {
                        writer.AppendLine("var mappers = settings?.Mappers ?? DynamicsMapperSettings.Default.Mappers;");
                        writer.AppendLine($"Entity? entity;");
                        using (writer.BeginScope($"if (string.IsNullOrEmpty(alias))"))
                        {
                            writer.AppendLine($"entity = source;");
                        }
                        using (writer.BeginScope($"else"))
                        {
                            writer.AppendLine($"entity = source.GetAliasedEntity(alias);");
                            writer.AppendLine($"if (entity is null) return null;");
                        }
                        writer.AppendLine($"if (entity?.LogicalName != entityname)");
                        writer.AppendLine($"throw new ArgumentException($\"entity LogicalName expected to be {{entityname}} recived: {{entity?.LogicalName}}\",\"entity\");");
                        writer.AppendLine($"var {modelName} = new {className}();");
                        writer.AppendLine(toModelContent.ToString());
                        writer.AppendLine($"return {modelName};");
                    }
                }
            }
            return new MapperClassDetails
            {
                Code = writer.ToString(),
                ClassName = mapperClassName
            };
        }

        private static string GenerateDynamicsmappingClass(string className, FieldGenerationDetails[] generationDetails, out string targetsClassName)
        {
            targetsClassName = $"{className}Targets";
            var writer = new CodeWriter();
            var dynamicLookupsAttributes = generationDetails.Where(gd => gd.Mapping == MappingType.DynamicLookup);
            using (writer.BeginScope($"public class {targetsClassName}"))
            {
                var properties = new List<string>();
                foreach (var attribute in dynamicLookupsAttributes)
                {
                    writer.AppendLine($"public string {attribute.PropertySymbol.Name} {{ get; set; }} = string.Empty;");
                }
                using (writer.BeginScope($"public static implicit operator DynamicsMappingsTargets({className}Targets targets)"))
                {
                    using (writer.BeginScope($"return new DynamicsMappingsTargets"))
                    {
                        foreach (var attribute in dynamicLookupsAttributes)
                        {
                            writer.AppendLine($"{{\"{attribute.SchemaName}\", targets.{attribute.PropertySymbol.Name} }},");
                        }
                    }
                    writer.AppendLine(";");
                }
            }
            return writer.ToString();
        }

        private static Mappings? GetMapperContent(string modelName, FieldGenerationDetails attribute, ICollection<MapperDetails> createdMappers, SourceProductionContext ctx)
        {
            return attribute.Mapping switch
            {
                MappingType.Basic => GenerateBasicMappings(modelName, attribute, ctx),
                MappingType.Lookup => GenerateLookupMappings(modelName, attribute, ctx),
                MappingType.Money => GenerateMoneyMappings(modelName, attribute, ctx),
                MappingType.Formatted => GenerateFormattedMappings(modelName, attribute, ctx),
                MappingType.MultipleOptions => GenerateMultipleOptionsMappings(modelName, attribute, ctx),
                MappingType.Options => GenerateOptionsMappings(modelName, attribute, ctx),
                MappingType.PrimaryId => GeneratePrimaryIdMappings(modelName, attribute, ctx),
                MappingType.Link => GenerateLinkMappings(modelName, attribute, createdMappers, ctx),
                MappingType.DynamicLookup => throw new Exception("Dynamic Lookups should be resolved last."),
                MappingType.DynamicLookupTarget => GenerateDynamicLookupTargetMappings(modelName, attribute, ctx),
                _ => throw new Exception($"{attribute.Mapping} is not defined"),
            };
        }

        private static Mappings? GenerateDynamicLookupMappings(string modelName, string? correspondingTargetLookup, FieldGenerationDetails attribute, SourceProductionContext ctx)
        {
            var allowedTypes = GetAllowedTypes(MappingType.DynamicLookup);
            var typeSymbol = attribute.PropertySymbol.Type.GetUnelyingType().Name;
            if (!allowedTypes.Any(t => t.Name == typeSymbol))
            {
                attribute.PropertySymbol.SetInvalidTypeDiagnostic(ctx, typeSymbol, MappingType.DynamicLookup, allowedTypes);
                return null;
            }
            string toModel;
            var toEntity = new StringBuilder();
            var target = attribute.Target;

            var attributeType = attribute.PropertySymbol.Type.ToString();
            var attributeRef = $"{modelName}.{attribute.PropertySymbol.Name}";

            if (!string.IsNullOrEmpty(correspondingTargetLookup))
            {
                toEntity.AppendLine($"string {attribute.SchemaName}_target;");
                toEntity.AppendLine($"if (!string.IsNullOrEmpty({correspondingTargetLookup}))");
                toEntity.AppendLine($"{attribute.SchemaName}_target = {correspondingTargetLookup};");
                toEntity.AppendLine($"else if (dynamicMappingsTargets?.TryGetValue(\"{attribute.SchemaName}\",out {attribute.SchemaName}_target) != true || string.IsNullOrEmpty({attribute.SchemaName}_target))");
            }
            else
            {
                toEntity.AppendLine($"if (dynamicMappingsTargets?.TryGetValue(\"{attribute.SchemaName}\",out var {attribute.SchemaName}_target) != true || string.IsNullOrEmpty({attribute.SchemaName}_target))");
            }
            toEntity.AppendLine($"throw new ArgumentException(\"target not found for '{attribute.SchemaName}'\",nameof(dynamicMappingsTargets));");
            if (attribute.PropertySymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                toModel = $"{attributeRef} = mappers.LookupMapper.MapToModel(entity, \"{attribute.SchemaName}\");";
                toEntity.AppendLine($"entity[\"{attribute.SchemaName}\"] = mappers.LookupMapper.MapToEntity({attributeRef},{attribute.SchemaName}_target);");
            }
            else
            {
                toModel = $"{attributeRef} = mappers.LookupMapper.MapToModel(entity, \"{attribute.SchemaName}\") ?? Guid.Empty;";
                toEntity.AppendLine($"entity[\"{attribute.SchemaName}\"] = mappers.LookupMapper.MapToEntity({attributeRef},{attribute.SchemaName}_target);");
            }
            return new Mappings(toModel, toEntity.ToString(), attributeType, attributeRef, MappingType.DynamicLookup, attribute);
        }

        private static Mappings? GenerateDynamicLookupTargetMappings(string modelName, FieldGenerationDetails attribute, SourceProductionContext ctx)
        {
            var allowedTypes = GetAllowedTypes(MappingType.DynamicLookupTarget);
            var typeSymbol = attribute.PropertySymbol.Type.GetUnelyingType().Name;
            if (!allowedTypes.Any(t => t.Name == typeSymbol))
            {
                attribute.PropertySymbol.SetInvalidTypeDiagnostic(ctx, typeSymbol, MappingType.DynamicLookupTarget, allowedTypes);
                return null;
            }
            var attributeType = attribute.PropertySymbol.Type.ToString();
            var attributeRef = $"{modelName}.{attribute.PropertySymbol.Name}";

            var nullable = attribute.PropertySymbol.NullableAnnotation == NullableAnnotation.Annotated;
            string toModel;
            if (nullable)
            {
                toModel = $"{attributeRef} = mappers.DynamicLookupTargetMapper.MapToModel(entity,\"{attribute.SchemaName}\");";
            }
            else
            {
                toModel = $"{attributeRef} = mappers.DynamicLookupTargetMapper.MapToModel(entity,\"{attribute.SchemaName}\") ?? string.Empty;";
            }
            return new Mappings(toModel, string.Empty, attributeType, attributeRef, MappingType.DynamicLookupTarget, attribute);
        }

        private static Mappings? GenerateLinkMappings(string modelName, FieldGenerationDetails attribute, ICollection<MapperDetails> createdMappers, SourceProductionContext ctx)
        {
            var typeSymbol = attribute.PropertySymbol.Type.GetUnelyingType().Name;
            var syntaxReference = attribute.PropertySymbol.Type.DeclaringSyntaxReferences.FirstOrDefault()
                ?? throw new Exception("syntaxReference not found");
            if (syntaxReference.GetSyntax() is not ClassDeclarationSyntax mapperSyntax)
                throw new Exception("syntax is not  ClassDeclarationSyntax");

            var mapperNameSpace = mapperSyntax.GetParent<BaseNamespaceDeclarationSyntax>()!.Name.ToString();
            var foundLinkDetails = createdMappers.Where(m => m.ClassDeclarationSyntax == mapperSyntax);
            if (foundLinkDetails.Count() != 1)
            {
                attribute.PropertySymbol.SetDiagnostic(ctx, DiagnosticsDescriptors.DestinationMapperNotFound, attribute.PropertySymbol.ToDisplayString());
                return null;

            }
            var attributeType = attribute.PropertySymbol.Type.ToString();
            var attributeRef = $"{modelName}.{attribute.PropertySymbol.Name}";

            var linkDetails = foundLinkDetails.Single();

            string toModel;
            var cw = new CodeWriter();
            var nullable = attribute.PropertySymbol.NullableAnnotation == NullableAnnotation.Annotated;
            var mapperName = $"{char.ToLower(linkDetails.MapperClassName[0])}{linkDetails.MapperClassName.Substring(1)}";
            cw.AppendLine($"var {mapperName} = new {mapperNameSpace}.{linkDetails.MapperClassName}();");

            cw.AppendLine($"var mapped_{linkDetails.EntityName} = {mapperName}.Map(source, \"{attribute.Alias}\");");
            cw.AppendLine($"if (mapped_{linkDetails.EntityName} != null)");
            cw.AppendLine($"{modelName}.{attribute.PropertySymbol.Name} = mapped_{linkDetails.EntityName};");
            toModel = cw.ToString();
            return new Mappings(toModel, string.Empty, string.Empty, attributeRef, MappingType.Link, attribute);
        }

        private static Mappings? GeneratePrimaryIdMappings(string modelName, FieldGenerationDetails attribute, SourceProductionContext ctx)
        {
            var allowedTypes = GetAllowedTypes(MappingType.PrimaryId);
            var typeSymbol = attribute.PropertySymbol.Type.GetUnelyingType().Name;
            var nullable = attribute.PropertySymbol.NullableAnnotation == NullableAnnotation.Annotated;
            if (!allowedTypes.Any(t => t.Name == typeSymbol))
            {
                attribute.PropertySymbol.SetInvalidTypeDiagnostic(ctx, typeSymbol, MappingType.Basic, allowedTypes);
                return null;
            }
            string toModel;
            var attributeType = attribute.PropertySymbol.Type.ToString();
            var attributeRef = $"{modelName}.{attribute.PropertySymbol.Name}";

            var toEntity = $"entity.Id = mappers.PrimaryIdMapper.MapToEntity({attributeRef}) ?? Guid.Empty;";
            if (nullable)
            {
                toModel = $"{attributeRef} = mappers.PrimaryIdMapper.MapToModel(entity, \"{attribute.SchemaName}\");";
            }
            else
            {
                toModel = $"{attributeRef} = mappers.PrimaryIdMapper.MapToModel(entity, \"{attribute.SchemaName}\") ?? Guid.Empty;";
            }
            return new Mappings(toModel, toEntity, attributeType, attributeRef, MappingType.PrimaryId, attribute);
        }

        private static Mappings? GenerateOptionsMappings(string modelName, FieldGenerationDetails attribute, SourceProductionContext ctx)
        {
            var unelyingTypeSymbol = attribute.PropertySymbol.Type.GetUnelyingType();
            if (unelyingTypeSymbol.Name != typeof(int).Name && unelyingTypeSymbol.TypeKind != TypeKind.Enum)
            {
                attribute.PropertySymbol.SetInvalidTypeDiagnostic(ctx, unelyingTypeSymbol.Name, MappingType.Options, new[] { typeof(int).Name, typeof(Enum).Name });
                return null;
            }
            string toEntity;
            string toModel;
            var nullable = attribute.PropertySymbol.NullableAnnotation == NullableAnnotation.Annotated;
            if (attribute.PropertySymbol.Type is not INamedTypeSymbol typeSymbol)
                throw new Exception("INamedTypeSymbol is expected");
            var targetType = typeSymbol;
            if (nullable)
                targetType = typeSymbol.TypeArguments.First() as INamedTypeSymbol;

            var castNeeded = targetType!.Name != typeof(int).Name;
            var modelAsInt = castNeeded ? $"(int?){modelName}.{attribute.PropertySymbol.Name}" : $"{modelName}.{attribute.PropertySymbol.Name}";

            var attributeType = typeSymbol.ToDisplayString();
            var attributeRef = $"{modelName}.{attribute.PropertySymbol.Name}";

            if (nullable)
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = mappers.OptionsetMapper.MapToEntity({modelAsInt});";
                if (castNeeded)
                    toModel = $"{attributeRef} = ({attributeType})mappers.OptionsetMapper.MapToModel(entity, \"{attribute.SchemaName}\");";
                else
                    toModel = $"{attributeRef} = mappers.OptionsetMapper.MapToModel(entity, \"{attribute.SchemaName}\");";
            }
            else
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = mappers.OptionsetMapper.MapToEntity({modelAsInt});";
                if (castNeeded)
                    toModel = $"{attributeRef} = ({attributeType})(mappers.OptionsetMapper.MapToModel(entity, \"{attribute.SchemaName}\") ?? 0);";
                else
                    toModel = $"{attributeRef} = mappers.OptionsetMapper.MapToModel(entity, \"{attribute.SchemaName}\") ?? 0;";
            }
            return new Mappings(toModel, toEntity, attributeType, attributeRef, MappingType.Options, attribute);
        }

        private static Mappings? GenerateMultipleOptionsMappings(string modelName, FieldGenerationDetails attribute, SourceProductionContext ctx)
        {
            string toEntity;
            string toModel;
            var nullable = attribute.PropertySymbol.NullableAnnotation == NullableAnnotation.Annotated;
            if (attribute.PropertySymbol.Type is not IArrayTypeSymbol arrayTypeSymbol)
                throw new Exception("IArrayTypeSymbol is expected");

            if (arrayTypeSymbol.ElementType is not INamedTypeSymbol elementTypeSymbol)
                throw new Exception("INamedTypeSymbol is expected");

            if (elementTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                attribute.PropertySymbol.SetDiagnostic(ctx, DiagnosticsDescriptors.NullableElementOnMultipleOptions);
                return null;
            }
            var unelyingTypeSymbol = elementTypeSymbol.GetUnelyingType();
            if (unelyingTypeSymbol.Name != typeof(int).Name && unelyingTypeSymbol.TypeKind != TypeKind.Enum)
            {
                attribute.PropertySymbol.SetInvalidTypeDiagnostic(ctx, unelyingTypeSymbol.Name, MappingType.MultipleOptions, new[] { typeof(int).Name, typeof(Enum).Name });
                return null;
            }
            var elementCastNeeded = elementTypeSymbol!.Name != typeof(int).Name;
            var elementCastString = elementCastNeeded ? "(int)" : string.Empty;

            var attributeType = attribute.PropertySymbol.Type.ToString();
            var attributeRef = $"{modelName}.{attribute.PropertySymbol.Name}";

            if (nullable)
            {
                if (elementCastNeeded)
                {
                    toEntity = $"entity[\"{attribute.SchemaName}\"] = mappers.OptionSetValueCollectionMapper.MapToEntity<{elementTypeSymbol.ToDisplayString()}>({attributeRef});";
                    toModel = $"{attributeRef} = mappers.OptionSetValueCollectionMapper.MapToModel<{elementTypeSymbol.ToDisplayString()}>(entity, \"{attribute.SchemaName}\");";
                }
                else
                {
                    toEntity = $"entity[\"{attribute.SchemaName}\"] = mappers.OptionSetValueCollectionMapper.MapToEntity({elementCastString}{attributeRef});";
                    toModel = $"{attributeRef} = mappers.OptionSetValueCollectionMapper.MapToModel(entity, \"{attribute.SchemaName}\");";
                }
            }
            else
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = mappers.OptionSetValueCollectionMapper.MapToEntity({attributeRef});";
                if (elementCastNeeded)
                    toModel = $"{attributeRef} = mappers.OptionSetValueCollectionMapper.MapToModel<{elementTypeSymbol.ToDisplayString()}>(entity, \"{attribute.SchemaName}\") ?? Array.Empty<{elementTypeSymbol.ToDisplayString()}>();";
                else
                    toModel = $"{attributeRef} = mappers.OptionSetValueCollectionMapper.MapToModel(entity, \"{attribute.SchemaName}\");";
            }
            return new Mappings(toModel, toEntity, attributeType, attributeRef, MappingType.MultipleOptions, attribute);
        }

        private static Mappings? GenerateFormattedMappings(string modelName, FieldGenerationDetails attribute, SourceProductionContext ctx)
        {
            var allowedTypes = GetAllowedTypes(MappingType.Formatted);
            var typeSymbol = attribute.PropertySymbol.Type.GetUnelyingType().Name;
            if (!allowedTypes.Any(t => t.Name == typeSymbol))
            {
                attribute.PropertySymbol.SetInvalidTypeDiagnostic(ctx, typeSymbol, MappingType.Formatted, allowedTypes);
                return null;
            }
            var attributeType = attribute.PropertySymbol.Type.ToString();
            var attributeRef = $"{modelName}.{attribute.PropertySymbol.Name}";

            var nullable = attribute.PropertySymbol.NullableAnnotation == NullableAnnotation.Annotated;
            string toModel;
            if (nullable)
            {
                toModel = $"{attributeRef} = mappers.FormattedValuesMapper.MapToModel(entity,\"{attribute.SchemaName}\");";
            }
            else
            {
                toModel = $"{attributeRef} = mappers.FormattedValuesMapper.MapToModel(entity,\"{attribute.SchemaName}\") ?? string.Empty;";
            }
            return new Mappings(toModel, string.Empty, attributeType, attributeRef, MappingType.Formatted, attribute);
        }

        private static Mappings? GenerateMoneyMappings(string modelName, FieldGenerationDetails attribute, SourceProductionContext ctx)
        {
            var allowedTypes = GetAllowedTypes(MappingType.Money);
            var typeSymbol = attribute.PropertySymbol.Type.GetUnelyingType().Name;
            if (!allowedTypes.Any(t => t.Name == typeSymbol))
            {
                attribute.PropertySymbol.SetInvalidTypeDiagnostic(ctx, typeSymbol, MappingType.Money, allowedTypes);
                return null;
            }
            string toModel;
            string toEntity;

            var attributeType = attribute.PropertySymbol.Type.ToString();
            var attributeRef = $"{modelName}.{attribute.PropertySymbol.Name}";

            if (attribute.PropertySymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = mappers.MoneyMapper.MapToEntity({attributeRef});";
                toModel = $"{attributeRef} = mappers.MoneyMapper.MapToModel(entity, \"{attribute.SchemaName}\");";
            }
            else
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = mappers.MoneyMapper.MapToEntity({attributeRef});";
                toModel = $"{modelName}.{attribute.PropertySymbol.Name} = mappers.MoneyMapper.MapToModel(entity, \"{attribute.SchemaName}\") ?? 0m;";
            }
            return new Mappings(toModel, toEntity, attributeType, attributeRef, MappingType.Money, attribute);
        }

        private static Mappings? GenerateLookupMappings(string modelName, FieldGenerationDetails attribute, SourceProductionContext ctx)
        {
            var allowedTypes = GetAllowedTypes(MappingType.Lookup);
            var typeSymbol = attribute.PropertySymbol.Type.GetUnelyingType().Name;
            if (!allowedTypes.Any(t => t.Name == typeSymbol))
            {
                attribute.PropertySymbol.SetInvalidTypeDiagnostic(ctx, typeSymbol, MappingType.Lookup, allowedTypes);
                return null;
            }

            var attributeType = attribute.PropertySymbol.Type.ToString();
            var attributeRef = $"{modelName}.{attribute.PropertySymbol.Name}";

            string toModel;
            string toEntity;
            if (attribute.PropertySymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                toModel = $"{attributeRef} = mappers.LookupMapper.MapToModel(entity, \"{attribute.SchemaName}\");";
                toEntity = $"entity[\"{attribute.SchemaName}\"] = mappers.LookupMapper.MapToEntity({attributeRef},\"{attribute.Target}\");";
            }
            else
            {
                toModel = $"{attributeRef} = mappers.LookupMapper.MapToModel(entity, \"{attribute.SchemaName}\") ?? Guid.Empty;";
                toEntity = $"entity[\"{attribute.SchemaName}\"] = mappers.LookupMapper.MapToEntity({attributeRef},\"{attribute.Target}\");";
            }
            return new Mappings(toModel, toEntity, attributeType, attributeRef, MappingType.Lookup, attribute);
        }
        private static Mappings? GenerateBasicMappings(string modelName, FieldGenerationDetails attribute, SourceProductionContext ctx)
        {
            var allowedTypes = GetAllowedTypes(MappingType.Basic);
            var typeSymbol = attribute.PropertySymbol.Type.GetUnelyingType().Name;

            var attributeType = attribute.PropertySymbol.Type.ToString();
            var attributeRef = $"{modelName}.{attribute.PropertySymbol.Name}";

            if (!allowedTypes.Any(t => t.Name == typeSymbol))
            {
                attribute.PropertySymbol.SetInvalidTypeDiagnostic(ctx, typeSymbol, MappingType.Basic, allowedTypes);
                return null;
            }
            var toModel = $"{attributeRef} = mappers.BasicMapper.MapToModel<{attributeType}>(entity, \"{attribute.SchemaName}\");";
            var toEntity = $"entity[\"{attribute.SchemaName}\"] = mappers.BasicMapper.MapToEntity<{attributeType}>({attributeRef});";
            return new Mappings(toModel, toEntity, attributeType, attributeRef, MappingType.Basic, attribute);
        }

        private static IEnumerable<FieldGenerationDetails> ExtractAttributes(IEnumerable<IPropertySymbol> properties, INamedTypeSymbol crmFieldAttributeSymbol, INamedTypeSymbol crmLinkAttributeSymbol, SourceProductionContext ctx)
        {
            foreach (var propertySymbol in properties)
            {
                if (propertySymbol.HasAttribute(crmLinkAttributeSymbol))
                {
                    var attributeData = propertySymbol.GetAttribute(crmLinkAttributeSymbol).First();
                    var alias = (string)attributeData.ConstructorArguments[0].Value!;
                    yield return FieldGenerationDetails.CreateAlias(propertySymbol, alias);
                }
                if (propertySymbol.HasAttribute(crmFieldAttributeSymbol))
                {
                    var attributeData = propertySymbol.GetAttribute(crmFieldAttributeSymbol).First();
                    var logicalName = (string)attributeData.ConstructorArguments[0].Value!;
                    var mappingType = MappingType.Basic;
                    string? target = null;
                    foreach (var namedArgument in attributeData.NamedArguments)
                    {
                        if (namedArgument.Key == "Mapping")
                            mappingType = (MappingType)namedArgument.Value.Value!;
                        if (namedArgument.Key == "Target")
                        {
                            target = (string)namedArgument.Value.Value!;
                            mappingType = MappingType.Lookup;
                        }
                    }

                    if (string.IsNullOrEmpty(logicalName))
                    {
                        propertySymbol.SetDiagnostic(ctx, DiagnosticsDescriptors.NoLogicalname, propertySymbol.Name);
                        continue;
                    }

                    yield return new FieldGenerationDetails(logicalName, propertySymbol, mappingType, target);
                }
            }
        }

        private static Type[] GetAllowedTypes(MappingType mappingType)
        {
            return mappingType switch
            {
                MappingType.Basic => [typeof(bool), typeof(Guid), typeof(int), typeof(DateTime), typeof(double), typeof(float), typeof(decimal), typeof(string), typeof(long)],
                MappingType.Lookup => [typeof(Guid)],
                MappingType.Money => [typeof(decimal)],
                MappingType.Formatted => [typeof(string)],
                MappingType.Options => [typeof(int)],
                MappingType.PrimaryId => [typeof(Guid)],
                MappingType.MultipleOptions => [],
                MappingType.DynamicLookup => [typeof(Guid)],
                MappingType.DynamicLookupTarget => [typeof(string)],
                _ => throw new Exception($"Unknown mapping type: {mappingType}"),
            };
        }
    }
}
