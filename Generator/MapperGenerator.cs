﻿using DynamicsMapper.Abstractions;
using DynamicsMapper.Attributes;
using DynamicsMapper.Extentions;
using DynamicsMapper.Helpers;
using DynamicsMapper.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design.Serialization;
using System.Linq;

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

            var crmEntityAttributeSymbol = compilation.GetTypeByMetadataName(typeof(CrmEntityAttribute).FullName);
            var crmFieldAttributeSymbol = compilation.GetTypeByMetadataName(typeof(CrmFieldAttribute).FullName);

            if (crmFieldAttributeSymbol == null)
                return;


            if (crmEntityAttributeSymbol == null)
                return;

            foreach (var mapperSyntax in mappers.Distinct())
            {
                var mapperModel = compilation.GetSemanticModel(mapperSyntax.SyntaxTree);
                if (mapperModel.GetDeclaredSymbol(mapperSyntax) is not INamedTypeSymbol mapperSymbol)
                    continue;

                var crmAttributeData = mapperSymbol.GetAttribute(crmEntityAttributeSymbol).FirstOrDefault();
                if (crmAttributeData is null)
                    continue;

                if (!mapperSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                {
                    mapperSymbol.SetDiagnostic(ctx, DiagnosticsDescriptors.NotPartial, mapperSymbol.Name);
                    continue;
                }

                var entityName = (string)crmAttributeData.ConstructorArguments[0].Value!;
                var properties = mapperSymbol.GetMembers().OfType<IPropertySymbol>();

                var generationDetails = ExtractAttributes(properties, crmFieldAttributeSymbol, ctx)
                    .ToArray();

                var duplicates = generationDetails.Where(dg => dg.Mapping != MappingType.Formatted)
                    .GroupBy(dg => dg.SchemaName)
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g);
                    

                foreach (var duplicate in duplicates)
                    mapperSymbol.SetDiagnostic(ctx, DiagnosticsDescriptors.DuplicateSchemas, mapperSymbol.Name);

                if (generationDetails.Where(gd => gd.Mapping == MappingType.PrimaryId).Count() > 1)
                {
                    mapperSymbol.SetDiagnostic(ctx, DiagnosticsDescriptors.MultiplePrimaryIds, mapperSymbol.Name);
                    continue;
                }
                var className = mapperSyntax.Identifier.ValueText;
                var classContent = GeneratePartialMapperClass(mapperSyntax, entityName, generationDetails, ctx);

                // verify compilation
                classContent = SyntaxFactory
                      .ParseCompilationUnit(classContent)
                      .NormalizeWhitespace()
                      .GetText()
                      .ToString();
                ctx.AddSource($"{className}.g.cs", classContent);
            }
        }

        private static string GeneratePartialMapperClass(ClassDeclarationSyntax mapperSyntax, string entityName, FieldGenerationDetails[] generationDetails, SourceProductionContext ctx)
        {
            var @namespace = mapperSyntax.GetParent<NamespaceDeclarationSyntax>()!.Name.ToString();
            var className = mapperSyntax.Identifier.ValueText;
            var columns = generationDetails.Select(a => $"\"{a.SchemaName}\"").Distinct().ToList();
            var writer = new CodeWriter();
            writer.AppendLine("// <auto-generated />");
            writer.AppendLine("#nullable enable");
            writer.AppendLine();
            writer.AddUsing("Microsoft.Xrm.Sdk");
            writer.AddUsing("Microsoft.Xrm.Sdk.Query");
            writer.AppendLine();
            var modelName = $"{char.ToLower(className[0])}{className.Substring(1)}";
            var toEntityContent = new List<string>();
            var toModelContent = new List<string>();

            foreach (var attribute in generationDetails)
            {
                var mappings = GetMapperContent(modelName, attribute, ctx);
                var hasSetter = attribute.PropertySymbol.SetMethod is not null;

                if (!mappings.HasValue)
                    continue;
                toEntityContent.Add(mappings.Value.ToEntity);
                if (hasSetter)
                    toModelContent.Add(mappings.Value.ToModel);
            }
            using (writer.BeginScope($"namespace {@namespace}"))
            {
                using (writer.BeginScope($"public partial class {className}"))
                {
                    writer.AppendLine($"public static ColumnSet ColumnSet = new ColumnSet({string.Join(", ", columns)});");
                    writer.AppendLine();
                    using (writer.BeginScope($"public Entity ToEntity()"))
                    {
                        writer.AppendLine($"var entity = new Entity(\"{entityName}\");");
                        toEntityContent.ForEach(writer.AppendLine);
                        writer.AppendLine("return entity;");
                    }
                    writer.AppendLine();
                    using (writer.BeginScope($"public static {className} FromEntity(Entity entity)"))
                    {
                        writer.AppendLine($"var {modelName} = new {className}();");
                        toModelContent.ForEach(writer.AppendLine);
                        writer.AppendLine($"return {modelName};");
                    }
                }
            }
            return writer.ToString();
        }

        private static Mappings? GetMapperContent(string modelName, FieldGenerationDetails attribute, SourceProductionContext ctx)
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
                _ => throw new Exception($"{attribute.Mapping} is not defined"),
            };
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
            var toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.Id;";
            if (nullable)
            {
                toEntity = $"entity.Id = {attribute.PropertySymbol.Name}.HasValue ? {attribute.PropertySymbol.Name}.Value : Guid.Empty;";
            }
            else
            {
                toEntity = $"entity.Id = {attribute.PropertySymbol.Name};";
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
            var modelAsInt = castNeeded ? $"(int){attribute.PropertySymbol.Name}" : attribute.PropertySymbol.Name;
            if (nullable)
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = {attribute.PropertySymbol.Name}.HasValue ? new OptionSetValue({modelAsInt}.Value) : null;";
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
                toEntity = $"entity[\"{attribute.SchemaName}\"] = {attribute.PropertySymbol.Name} is null ? null : new OptionSetValueCollection({attribute.PropertySymbol.Name}.Select(e => new OptionSetValue({elementCastString}e)).ToList());";
                if (elementCastNeeded)
                    toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<OptionSetValueCollection>(\"{attribute.SchemaName}\")?.Select(e => e.Value).Cast<{elementTypeSymbol.ToDisplayString()}>().ToArray();";
                else
                    toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<OptionSetValueCollection>(\"{attribute.SchemaName}\")?.Select(e => e.Value).ToArray();";
            }
            else
            {
                toEntity = $"entity[\"{attribute.SchemaName}\"] = new OptionSetValueCollection({attribute.PropertySymbol.Name}.Select(e => new OptionSetValue({elementCastString}e)).ToList());";
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
                toEntity = $"entity[\"{attribute.SchemaName}\"] = {attribute.PropertySymbol.Name}.HasValue ? new Money({attribute.PropertySymbol.Name}.Value) : null;";
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
                toEntity = $"entity[\"{attribute.SchemaName}\"] = {attribute.PropertySymbol.Name}.HasValue ? new EntityReference(\"{attribute.Target}\", {attribute.PropertySymbol.Name}.Value) : null;";
            }
            else
            {
                toModel = $"{modelName}.{attribute.PropertySymbol.Name} = entity.GetAttributeValue<EntityReference>(\"{attribute.SchemaName}\")?.Id ?? Guid.Empty;";
                toEntity = $"entity[\"{attribute.SchemaName}\"] = new EntityReference(\"{attribute.Target}\", {attribute.PropertySymbol.Name});";
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
            var toEntity = $"entity[\"{attribute.SchemaName}\"] = {attribute.PropertySymbol.Name};";
            return new Mappings(toModel, toEntity);
        }

        private static IEnumerable<FieldGenerationDetails> ExtractAttributes(IEnumerable<IPropertySymbol> properties, INamedTypeSymbol crmFieldAttributeSymbol, SourceProductionContext ctx)
        {
            foreach (var propertySymbol in properties)
            {
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
