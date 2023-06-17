﻿using Generator.Attributes;
using Generator.Extensions;
using Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Generator
{
    [Generator]
    public class MapperGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            GenerateClasses(context);

            var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => IsAttributeNode(node),
                transform: static (ctx, _) => GetGenerationAttribute(ctx))
                .Where(static n => n is not null);

            var compilation = context.CompilationProvider.Combine(provider.Collect());
            context.RegisterSourceOutput(
                compilation,
                (spc, source) => Execute(spc, source.Left, source.Right!));
        }

        private static void GenerateClasses(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "CrmEntityAttribute.g.cs",
                SourceText.From(SourceGenerationHelper.GenerateCrmEntityAttribute(), Encoding.UTF8)));

            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "CrmFieldAttribute.g.cs",
                SourceText.From(SourceGenerationHelper.GenerateCrmFieldAttribute(), Encoding.UTF8)));

            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "CrmReferenceAttribute.g.cs",
                SourceText.From(SourceGenerationHelper.GenerateCrmReferenceAttribute(), Encoding.UTF8)));

            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "CrmMoneyAttribute.g.cs",
                SourceText.From(SourceGenerationHelper.GenerateCrmMoneyAttribute(), Encoding.UTF8)));
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
               "CrmFormattedAttribute.g.cs",
               SourceText.From(SourceGenerationHelper.GenerateCrmFormattedAttribute(), Encoding.UTF8)));

            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
               "CrmOptionsAttribute.g.cs",
               SourceText.From(SourceGenerationHelper.GenerateCrmOptionsAttribute(), Encoding.UTF8)));

            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "FieldType.g.cs",
                SourceText.From(SourceGenerationHelper.GenerateFieldTypeEnum(), Encoding.UTF8)));

        }

        private void Execute(SourceProductionContext context, Compilation compilation, ImmutableArray<ClassGenerationDetails> generationDetails)
        {
            foreach (var generationDetail in generationDetails)
            {

                var attributes = ExtractAttributes(compilation, generationDetail.Class).ToList();
                var duplicates = attributes.GroupBy(a => a.PropertyName).Where(g => g.Count() > 1).Select(a => a.Key);
                if (duplicates.Any())
                    throw new Exception($"{string.Join(",", duplicates)} in {generationDetail.Class.Identifier} has duplicate attributes");

                var @namespace = generationDetail.Class.GetParent<NamespaceDeclarationSyntax>()!.Name.ToString();
                var className = generationDetail.Class.Identifier.ToString();
                var columns = attributes.Select(a => $"\"{a.SchemaName}\"").Distinct().ToList();
                var writer = new CodeWriter();
                writer.AppendLine("// <auto-generated>");
                writer.AppendLine("#nullable enable");
                writer.AppendLine();
                writer.AddUsing("Microsoft.Xrm.Sdk");
                writer.AddUsing("Microsoft.Xrm.Sdk.Query");
                writer.AppendLine();
                var modelName = $"{char.ToLower(className[0])}{className.Substring(1)}";
                var toEntityContent = new List<string>();
                var toModelContent = new List<string>()
                {
                    $"{modelName}.{className}Id = entity.Id;"
                };

                foreach (var attribute in attributes)
                {
                    switch (attribute.FieldType)
                    {
                        case FieldType.Regular:
                            toModelContent.Add($"{modelName}.{attribute.PropertyName} = entity.GetAttributeValue<{attribute.PropertyType}>(\"{attribute.SchemaName}\");");
                            toEntityContent.Add($"entity[\"{attribute.SchemaName}\"] = {attribute.PropertyName};");
                            break;
                        case FieldType.Lookup:
                            if (attribute.Nullable)
                            {
                                toEntityContent.Add($"entity[\"{attribute.SchemaName}\"] = {attribute.PropertyName}.HasValue ? new EntityReference(\"{attribute.Target}\", {attribute.PropertyName}.Value) : null;");
                                toModelContent.Add($"{modelName}.{attribute.PropertyName} = entity.GetAttributeValue<EntityReference>(\"{attribute.SchemaName}\")?.Id;");
                            }
                            else
                            {
                                toModelContent.Add($"{modelName}.{attribute.PropertyName} = entity.GetAttributeValue<EntityReference>(\"{attribute.SchemaName}\")?.Id ?? Guid.Empty;");
                                toEntityContent.Add($"entity[\"{attribute.SchemaName}\"] = new EntityReference(\"{attribute.Target}\", {attribute.PropertyName});");
                            }
                            break;
                        case FieldType.Money:
                            if (attribute.Nullable)
                            {
                                toEntityContent.Add($"entity[\"{attribute.SchemaName}\"] = {attribute.PropertyName}.HasValue ? new Money({attribute.PropertyName}.Value) : null;");
                                toModelContent.Add($"{modelName}.{attribute.PropertyName} = entity.GetAttributeValue<Money>(\"{attribute.SchemaName}\")?.Value;");
                            }
                            else
                            {
                                toEntityContent.Add($"entity[\"{attribute.SchemaName}\"] = new Money({attribute.PropertyName});");
                                toModelContent.Add($"{modelName}.{attribute.PropertyName} = entity.GetAttributeValue<Money>(\"{attribute.SchemaName}\")?.Value ?? 0m;");
                            }
                            break;
                        case FieldType.Formatted:
                            toModelContent.Add($"if (entity.FormattedValues.TryGetValue(\"{attribute.SchemaName}\", out var formatted{attribute.PropertyName}))");
                            toModelContent.Add($"\t{modelName}.{attribute.PropertyName} = formatted{attribute.PropertyName};");
                            break;
                        case FieldType.Options:
                            var isInt = attribute.PropertyType == "int" || attribute.PropertyType == "int?";
                            string modelIntValue;
                            if (isInt)
                                modelIntValue = attribute.PropertyName;
                            else
                                modelIntValue = $"(int){attribute.PropertyName}";

                            if (attribute.Nullable)
                            {
                                toEntityContent.Add($"entity[\"{attribute.SchemaName}\"] = {attribute.PropertyName}.HasValue ? new OptionSetValue({modelIntValue}.Value) : null;");
                                toModelContent.Add($"{modelName}.{attribute.PropertyName} = ({attribute.PropertyType})(entity.GetAttributeValue<OptionSetValue>(\"{attribute.SchemaName}\")?.Value);");
                            }
                            else
                            {
                                toEntityContent.Add($"entity[\"{attribute.SchemaName}\"] = new OptionSetValue({modelIntValue});");
                                toModelContent.Add($"{modelName}.{attribute.PropertyName} = ({attribute.PropertyType})(entity.GetAttributeValue<OptionSetValue>(\"{attribute.SchemaName}\")?.Value ?? 0);");
                            }
                            break;
                        default:
                            throw new Exception($"{attribute.FieldType} is not defined");
                    }
                }
                using (writer.BeginScope($"namespace {@namespace}"))
                {
                    using (writer.BeginScope($"public partial class {className}"))
                    {
                        writer.AppendLine($"public static ColumnSet ColumnSet = new ColumnSet({string.Join(", ", columns)});");
                        writer.AppendLine();
                        writer.AppendLine($"public Guid {className}Id {{ get; set; }}");
                        writer.AppendLine();
                        using (writer.BeginScope($"public Entity ToEntity()"))
                        {
                            writer.AppendLine($"var entity = new Entity(\"{generationDetail.EntityName}\", {className}Id);");
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
                var gen = writer.ToString();
                context.AddSource($"{className}.g.cs", writer.ToString());
            }
        }

        private IEnumerable<FieldGenerationDetails> ExtractAttributes(Compilation compilation, ClassDeclarationSyntax @class)
        {
            var crmFields = ExtractCRMFieldAttributes(compilation, @class);
            var lookupFields = ExtractCRMReferenceAttributes(compilation, @class);
            var moneyFields = ExtractCRMMoneyAttributes(compilation, @class);
            var formattedAttributes = ExtractCRMFormattedAttributes(compilation, @class);
            var optionsAttributes = ExtractCRMOptionsAttributes(compilation, @class);

            return crmFields.Union(lookupFields)
                            .Union(moneyFields)
                            .Union(formattedAttributes)
                            .Union(optionsAttributes)
                            .OrderBy(fg => fg.PropertyName);
        }

        private IEnumerable<FieldGenerationDetails> ExtractCRMOptionsAttributes(Compilation compilation, ClassDeclarationSyntax @class)
        {
            var allowedTypes = new[] { "Int32" };
            var attributes = @class.DescendantNodes().OfType<AttributeSyntax>()
                .Where(syntax => syntax.Name.ToString() == "CrmOptions")
                .ToList();

            var errors = new List<string>();
            foreach (var attributeSyntax in attributes)
            {
                var propertyDeclarationSyntax = attributeSyntax.GetParent<PropertyDeclarationSyntax>()!;
                var propertySymbol = compilation
                    .GetSemanticModel(propertyDeclarationSyntax.SyntaxTree)
                    .GetDeclaredSymbol(propertyDeclarationSyntax)!;

                var typeSymbol = SourceGenerationHelper.GetTypeSymbol(propertySymbol!, out var nullable);
                var attributeData = propertySymbol.GetAttributes().Where(a => a.AttributeClass!.MetadataName == "CrmOptionsAttribute").First();
                var logicalName = attributeData.ConstructorArguments.First().Value as string;

                if (!allowedTypes.Contains(typeSymbol.Name, StringComparer.OrdinalIgnoreCase)
                    && typeSymbol is INamedTypeSymbol { EnumUnderlyingType: null })
                    errors.Add($"{typeSymbol.Name} type is not allowed");

                var propertyName = propertySymbol.Name;
                if (string.IsNullOrEmpty(logicalName))
                {
                    errors.Add($"The property {propertyName} has no logicalName");
                    continue;
                }

                yield return new FieldGenerationDetails(logicalName!, propertyName, propertyDeclarationSyntax.Type.ToString(), nullable)
                {
                    FieldType = FieldType.Options
                };
            }
            if (errors.Any())
                throw new Exception("errors found");
        }

        private IEnumerable<FieldGenerationDetails> ExtractCRMFormattedAttributes(Compilation compilation, ClassDeclarationSyntax @class)
        {
            var allowedTypes = new[] { "String" };
            var attributes = @class.DescendantNodes().OfType<AttributeSyntax>()
                .Where(syntax => syntax.Name.ToString() == "CrmFormatted")
                .ToList();

            var errors = new List<string>();
            foreach (var attributeSyntax in attributes)
            {
                var propertyDeclarationSyntax = attributeSyntax.GetParent<PropertyDeclarationSyntax>()!;
                var propertySymbol = compilation
                    .GetSemanticModel(propertyDeclarationSyntax.SyntaxTree)
                    .GetDeclaredSymbol(propertyDeclarationSyntax)!;

                var typeSymbol = SourceGenerationHelper.GetTypeSymbol(propertySymbol!, out var nullable);
                var attributeData = propertySymbol.GetAttributes().Where(a => a.AttributeClass!.MetadataName == "CrmFormattedAttribute").First();
                var logicalName = attributeData.ConstructorArguments.First().Value as string;

                if (!allowedTypes.Contains(typeSymbol.Name, StringComparer.OrdinalIgnoreCase))
                    errors.Add($"{typeSymbol.Name} type is not allowed");

                var propertyName = propertySymbol.Name;
                if (string.IsNullOrEmpty(logicalName))
                {
                    errors.Add($"The property {propertyName} has no logicalName");
                    continue;
                }

                yield return new FieldGenerationDetails(logicalName!, propertyName, propertyDeclarationSyntax.Type.ToString(), nullable)
                {
                    FieldType = FieldType.Formatted
                };
            }
            if (errors.Any())
                throw new Exception("errors found");
        }

        private IEnumerable<FieldGenerationDetails> ExtractCRMMoneyAttributes(Compilation compilation, ClassDeclarationSyntax @class)
        {
            var allowedTypes = new[] { "Decimal" };
            var attributes = @class.DescendantNodes().OfType<AttributeSyntax>()
                .Where(syntax => syntax.Name.ToString() == "CrmMoney")
                .ToList();

            var errors = new List<string>();
            foreach (var attributeSyntax in attributes)
            {
                var propertyDeclarationSyntax = attributeSyntax.GetParent<PropertyDeclarationSyntax>()!;
                var propertySymbol = compilation
                    .GetSemanticModel(propertyDeclarationSyntax.SyntaxTree)
                    .GetDeclaredSymbol(propertyDeclarationSyntax)!;

                var typeSymbol = SourceGenerationHelper.GetTypeSymbol(propertySymbol!, out var nullable);
                var attributeData = propertySymbol.GetAttributes().Where(a => a.AttributeClass!.MetadataName == "CrmMoneyAttribute").First();
                var logicalName = attributeData.ConstructorArguments.First().Value as string;

                if (!allowedTypes.Contains(typeSymbol.Name, StringComparer.OrdinalIgnoreCase))
                    errors.Add($"{typeSymbol.Name} type is not allowed");

                var propertyName = propertySymbol.Name;
                if (string.IsNullOrEmpty(logicalName))
                {
                    errors.Add($"The property {propertyName} has no logicalName");
                    continue;
                }

                yield return new FieldGenerationDetails(logicalName!, propertyName, propertyDeclarationSyntax.Type.ToString(), nullable)
                {
                    FieldType = FieldType.Money
                };
            }
            if (errors.Any())
                throw new Exception("errors found");
        }

        private IEnumerable<FieldGenerationDetails> ExtractCRMReferenceAttributes(Compilation compilation, ClassDeclarationSyntax @class)
        {
            var allowedTypes = new[] { "Guid" };
            var attributes = @class.DescendantNodes().OfType<AttributeSyntax>()
                .Where(syntax => syntax.Name.GetText().ToString() == "CrmReference")
                .ToList();

            var errors = new List<string>();
            foreach (var attributeSyntax in attributes)
            {
                var propertyDeclarationSyntax = attributeSyntax.GetParent<PropertyDeclarationSyntax>()!;
                var propertySymbol = compilation
                    .GetSemanticModel(propertyDeclarationSyntax.SyntaxTree)
                    .GetDeclaredSymbol(propertyDeclarationSyntax)!;

                var typeSymbol = SourceGenerationHelper.GetTypeSymbol(propertySymbol!, out var nullable);
                var attributeData = propertySymbol.GetAttributes().Where(a => a.AttributeClass!.MetadataName == "CrmReferenceAttribute").First();
                var logicalName = attributeData.ConstructorArguments.ElementAt(0).Value as string;
                var target = attributeData.ConstructorArguments.ElementAt(1).Value as string;

                if (!allowedTypes.Contains(typeSymbol.Name, StringComparer.OrdinalIgnoreCase))
                    errors.Add($"{typeSymbol.Name} type is not allowed");

                var propertyName = propertySymbol.Name;
                if (string.IsNullOrEmpty(logicalName))
                {
                    errors.Add($"The property {propertyName} has no logicalName");
                    continue;
                }

                if (string.IsNullOrEmpty(target))
                {
                    errors.Add($"The property {propertyName} has no target");
                    continue;
                }

                yield return new FieldGenerationDetails(logicalName!, propertyName, propertyDeclarationSyntax.Type.ToString(), nullable)
                {
                    Target = target!,
                    FieldType = FieldType.Lookup
                };
            }
            if (errors.Any())
                throw new Exception("errors found");
        }
        private IEnumerable<FieldGenerationDetails> ExtractCRMFieldAttributes(Compilation compilation, ClassDeclarationSyntax @class)
        {
            var allowedTypes = new[] { "Boolean", "Guid", "Int32", "DateTime", "Double", "Decimal", "String" };
            var attributes = @class.DescendantNodes().OfType<AttributeSyntax>()
                .Where(syntax => syntax.Name.ToString() == "CrmField")
                .ToList();

            var errors = new List<string>();
            foreach (var attributeSyntax in attributes)
            {
                var propertyDeclarationSyntax = attributeSyntax.GetParent<PropertyDeclarationSyntax>()!;
                var propertySymbol = compilation
                    .GetSemanticModel(propertyDeclarationSyntax.SyntaxTree)
                    .GetDeclaredSymbol(propertyDeclarationSyntax)!;

                var typeSymbol = SourceGenerationHelper.GetTypeSymbol(propertySymbol!, out var nullable);
                var attributeData = propertySymbol.GetAttributes().Where(a => a.AttributeClass!.MetadataName == "CrmFieldAttribute").First();
                var logicalName = attributeData.ConstructorArguments.First().Value as string;

                if (!allowedTypes.Contains(typeSymbol.Name, StringComparer.OrdinalIgnoreCase))
                    errors.Add($"{typeSymbol.Name} type is not allowed");

                var propertyName = propertySymbol.Name;
                if (string.IsNullOrEmpty(logicalName))
                {
                    errors.Add($"The property {propertyName} has no logicalName");
                    continue;
                }

                yield return new FieldGenerationDetails(logicalName!, propertyName, propertyDeclarationSyntax.Type.ToString(), nullable)
                {
                    FieldType = FieldType.Regular
                };
            }
            if (errors.Any())
                throw new Exception("errors found");
        }

        private static ClassGenerationDetails? GetGenerationAttribute(GeneratorSyntaxContext ctx)
        {
            var attributeSyntax = (AttributeSyntax)ctx.Node;
            if (attributeSyntax.Name is IdentifierNameSyntax nameSyntax)
            {
                if (nameSyntax is { Identifier.Text: "CrmEntity" })
                {
                    var firstArgument = (attributeSyntax.ArgumentList?.Arguments.FirstOrDefault())
                        ?? throw new Exception("firstArgument is null");

                    if (firstArgument.Expression is not LiteralExpressionSyntax literalExpression)
                        throw new Exception("literalExpression not found");

                    if (literalExpression.Token.Value is not string entityName)
                        throw new Exception("entityName not found");
                    var classSyntax = attributeSyntax.GetParent<ClassDeclarationSyntax>();
                    if (classSyntax is null)
                        throw new Exception("class not found");

                    return new ClassGenerationDetails(entityName, classSyntax);
                }
            }
            return null;
        }

        private static bool IsAttributeNode(SyntaxNode node) => node is AttributeSyntax;
    }
}
