﻿using DynamicsMapper.Abstractions;
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
            GenerateIEntityMapper(ctx);
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

                var duplicateSchemas = fieldsGenerationDetails.Where(dg => dg.Mapping != MappingType.Formatted && !string.IsNullOrEmpty(dg.SchemaName))
                    .GroupBy(dg => dg.SchemaName!)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g);


                foreach (var duplicate in duplicateSchemas)
                    duplicate.PropertySymbol.SetDiagnostic(ctx, DiagnosticsDescriptors.DuplicateSchemas, details.MapperSymbol.Name, duplicate.SchemaName!);

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

        private static void GenerateIEntityMapper(SourceProductionContext ctx)
        {
            var writer = new CodeWriter();
            writer.AppendLine("// <auto-generated />");
            writer.AppendLine("#nullable enable");
            writer.AppendLine();
            writer.AddUsing("Microsoft.Xrm.Sdk");
            writer.AddUsing("Microsoft.Xrm.Sdk.Query");
            writer.AppendLine();
            using (writer.BeginScope($"namespace DynamicsMapper.Mappers"))
            {
                using (writer.BeginScope($"public interface IEntityMapper<T>"))
                {
                    writer.AppendLine("public string Entityname { get; }");
                    writer.AppendLine("public ColumnSet Columns { get; }");
                    writer.AppendLine("public T Map(Entity entity);");
                    writer.AppendLine("public T? Map(Entity entity, string alias);");
                    writer.AppendLine("public Entity Map(T model);");
                }
            }
            var classContent = SyntaxFactory
                                 .ParseCompilationUnit(writer.ToString())
                                 .NormalizeWhitespace()
                                 .GetText()
                                 .ToString();
            ctx.AddSource($"IEntityMapper.g.cs", classContent);
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
        private static MapperClassDetails GenerateMapperClass(ClassDeclarationSyntax mapperSyntax, string entityName, FieldGenerationDetails[] generationDetails, string mapperClassName, ICollection<MapperDetails> createdMappers, SourceProductionContext ctx)
        {
            var columns = generationDetails.Where(a => !string.IsNullOrEmpty(a.SchemaName)).Select(a => $"\"{a.SchemaName}\"").Distinct().ToList();
            var @namespace = mapperSyntax.GetParent<NamespaceDeclarationSyntax>()!.Name.ToString();

            var writer = new CodeWriter();
            writer.AppendLine("// <auto-generated />");
            writer.AppendLine("#nullable enable");
            writer.AppendLine();
            writer.AddUsing("Microsoft.Xrm.Sdk");
            writer.AddUsing("Microsoft.Xrm.Sdk.Query");
            writer.AddUsing("DynamicsMapper.Extension");
            writer.AddUsing("DynamicsMapper.Mappers");
            writer.AddUsing("System");
            writer.AppendLine();
            var className = mapperSyntax.Identifier.ValueText;
            var modelName = $"{char.ToLower(className[0])}{className.Substring(1)}";
            var toEntityContent = new List<string>();
            var toModelContent = new List<string>();

            foreach (var attribute in generationDetails)
            {
                var mappings = GetMapperContent(modelName, attribute, createdMappers, ctx);
                var hasSetter = attribute.PropertySymbol.SetMethod is not null;

                if (!mappings.HasValue)
                    continue;
                toEntityContent.Add(mappings.Value.ToEntity);
                if (hasSetter)
                    toModelContent.Add(mappings.Value.ToModel);
            }

            using (writer.BeginScope($"namespace {@namespace}"))
            {
                using (writer.BeginScope($"public class {mapperClassName} : IEntityMapper<{className}>"))
                {
                    writer.AppendLine($"private static readonly string[] columns = new[] {{{string.Join(", ", columns)}}};");
                    writer.AppendLine($"public ColumnSet Columns => new ColumnSet(columns);");
                    writer.AppendLine($"private const string entityname = \"{entityName}\";");
                    writer.AppendLine($"public string Entityname => entityname;");
                    writer.AppendLine();
                    using (writer.BeginScope($"public Entity Map({className} {modelName})"))
                    {
                        writer.AppendLine($"var entity = new Entity(entityname);");
                        toEntityContent.ForEach(writer.AppendLine);
                        writer.AppendLine("return entity;");
                    }
                    writer.AppendLine($"public {className}? Map(Entity entity, string alias) => InternalMap(entity, alias);");
                    writer.AppendLine($"public {className} Map(Entity entity) => InternalMap(entity)!;");

                    using (writer.BeginScope($"private static {className}? InternalMap(Entity source, string? alias = null)"))
                    {
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
                        toModelContent.ForEach(writer.AppendLine);
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
                _ => throw new Exception($"{attribute.Mapping} is not defined"),
            }; ;
        }

        private static Mappings? GenerateLinkMappings(string modelName, FieldGenerationDetails attribute, ICollection<MapperDetails> createdMappers, SourceProductionContext ctx)
        {
            var typeSymbol = attribute.PropertySymbol.Type.GetUnelyingType().Name;
            var syntaxReference = attribute.PropertySymbol.Type.DeclaringSyntaxReferences.FirstOrDefault()
                ?? throw new Exception("syntaxReference not found");
            if (syntaxReference.GetSyntax() is not ClassDeclarationSyntax mapperSyntax)
                throw new Exception("syntax is not  ClassDeclarationSyntax");

            var mapperNameSpace = mapperSyntax.GetParent<NamespaceDeclarationSyntax>()!.Name.ToString();
            var foundLinkDetails = createdMappers.Where(m => m.ClassDeclarationSyntax == mapperSyntax);
            if (foundLinkDetails.Count() != 1)
            {
                attribute.PropertySymbol.SetDiagnostic(ctx, DiagnosticsDescriptors.DestinationMapperNotFound, attribute.PropertySymbol.ToDisplayString());
                return null;

            }
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
            return new Mappings(toModel, string.Empty);
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
            string toEntity;
            var toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<Guid>(\"{attribute.SchemaName}\");";
            if (nullable)
            {
                toEntity = $"entity.Id = {modelName}.{attribute.PropertySymbol.Name}.HasValue ? {modelName}.{attribute.PropertySymbol.Name}.Value : Guid.Empty;";
            }
            else
            {
                toEntity = $"entity.Id = {modelName}.{attribute.PropertySymbol.Name};";
            }
            return new Mappings(toModel, toEntity);
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
            var modelAsInt = castNeeded ? $"(int){modelName}.{attribute.PropertySymbol.Name}" : $"{modelName}.{attribute.PropertySymbol.Name}";
            if (nullable)
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = {modelName}.{attribute.PropertySymbol.Name}.HasValue ? new OptionSetValue({modelAsInt}.Value) : null;";
                if (castNeeded)
                    toModel = $"{modelName}.{attribute.PropertySymbol.Name} = ({typeSymbol.ToDisplayString()})(entity.GetAttributeValue<OptionSetValue>(\"{attribute.SchemaName}\")?.Value);";
                else
                    toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<OptionSetValue>(\"{attribute.SchemaName}\")?.Value;";
            }
            else
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = new OptionSetValue({modelAsInt});";
                if (castNeeded)
                    toModel = $"{modelName}.{attribute.PropertySymbol.Name} = ({typeSymbol.ToDisplayString()})(entity.GetAttributeValue<OptionSetValue>(\"{attribute.SchemaName}\")?.Value ?? 0);";
                else
                    toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<OptionSetValue>(\"{attribute.SchemaName}\")?.Value ?? 0;";
            }
            return new Mappings(toModel, toEntity);
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
            if (nullable)
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = {modelName}.{attribute.PropertySymbol.Name} is null ? null : new OptionSetValueCollection({modelName}.{attribute.PropertySymbol.Name}.Select(e => new OptionSetValue({elementCastString}e)).ToList());";
                if (elementCastNeeded)
                    toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<OptionSetValueCollection>(\"{attribute.SchemaName}\")?.Select(e => e.Value).Cast<{elementTypeSymbol.ToDisplayString()}>().ToArray();";
                else
                    toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<OptionSetValueCollection>(\"{attribute.SchemaName}\")?.Select(e => e.Value).ToArray();";
            }
            else
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = new OptionSetValueCollection({modelName}.{attribute.PropertySymbol.Name}.Select(e => new OptionSetValue({elementCastString}e)).ToList());";
                if (elementCastNeeded)
                    toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<OptionSetValueCollection>(\"{attribute.SchemaName}\")?.Select(e => e.Value).Cast<{elementTypeSymbol.ToDisplayString()}>().ToArray() ?? Array.Empty<{elementTypeSymbol.ToDisplayString()}>();";
                else
                    toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<OptionSetValueCollection>(\"{attribute.SchemaName}\")?.Select(e => e.Value).ToArray() ?? Array.Empty<{elementTypeSymbol.ToDisplayString()}>();";
            }
            return new Mappings(toModel, toEntity);
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

            var codeWriter = new CodeWriter();
            using (codeWriter.BeginScope($"if (entity.FormattedValues.TryGetValue(\"{attribute.SchemaName}\", out var formatted{attribute.PropertySymbol.Name}))"))
            {
                codeWriter.AppendLine($"{modelName}.{attribute.PropertySymbol.Name} = formatted{attribute.PropertySymbol.Name};");
            }
            return new Mappings(codeWriter.ToString(), string.Empty);
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
            if (attribute.PropertySymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = {modelName}.{attribute.PropertySymbol.Name}.HasValue ? new Money({modelName}.{attribute.PropertySymbol.Name}.Value) : null;";
                toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<Money>(\"{attribute.SchemaName}\")?.Value;";
            }
            else
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = new Money({attribute.PropertySymbol.Name});";
                toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<Money>(\"{attribute.SchemaName}\")?.Value ?? 0m;";
            }
            return new Mappings(toModel, toEntity);
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
            string toModel;
            string toEntity;
            if (attribute.PropertySymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<EntityReference>(\"{attribute.SchemaName}\")?.Id;";
                toEntity = $"entity[\"{attribute.SchemaName}\"] = {modelName}.{attribute.PropertySymbol.Name}.HasValue ? new EntityReference(\"{attribute.Target}\", {modelName}.{attribute.PropertySymbol.Name}.Value) : null;";
            }
            else
            {
                toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<EntityReference>(\"{attribute.SchemaName}\")?.Id ?? Guid.Empty;";
                toEntity = $"entity[\"{attribute.SchemaName}\"] = new EntityReference(\"{attribute.Target}\", {modelName}.{attribute.PropertySymbol.Name});";
            }
            return new Mappings(toModel, toEntity);
        }
        private static Mappings? GenerateBasicMappings(string modelName, FieldGenerationDetails attribute, SourceProductionContext ctx)
        {
            var allowedTypes = GetAllowedTypes(MappingType.Basic);
            var typeSymbol = attribute.PropertySymbol.Type.GetUnelyingType().Name;
            if (!allowedTypes.Any(t => t.Name == typeSymbol))
            {
                attribute.PropertySymbol.SetInvalidTypeDiagnostic(ctx, typeSymbol, MappingType.Basic, allowedTypes);
                return null;
            }
            var toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<{attribute.PropertySymbol.Type}>(\"{attribute.SchemaName}\");";
            var toEntity = $"entity[\"{attribute.SchemaName}\"] = {modelName}.{attribute.PropertySymbol.Name};";
            return new Mappings(toModel, toEntity);
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
                MappingType.Basic => new[] { typeof(bool), typeof(Guid), typeof(int), typeof(DateTime), typeof(double), typeof(decimal), typeof(string) },
                MappingType.Lookup => new[] { typeof(Guid) },
                MappingType.Money => new[] { typeof(decimal) },
                MappingType.Formatted => new[] { typeof(string) },
                MappingType.Options => new[] { typeof(int) },
                MappingType.PrimaryId => new[] { typeof(Guid) },
                MappingType.MultipleOptions => Array.Empty<Type>(),
                _ => throw new Exception($"Unknown mapping type: {mappingType}"),
            };
        }
    }
}
