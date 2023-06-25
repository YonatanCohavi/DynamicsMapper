using DynamicsMapper.Abstractions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicsMapper.Helpers
{
    internal static class SymbolExtensions
    {
        internal static void SetDiagnostic(this ISymbol symbol, SourceProductionContext context, DiagnosticDescriptor diagnosticDescriptor, params object?[]? messageArgs) =>
            context.ReportDiagnostic(Diagnostic.Create(diagnosticDescriptor,
                                                       symbol.Locations.First(),
                                                       symbol.Locations.Skip(1),
                                                       messageArgs));

        internal static void SetInvalidTypeDiagnostic(this ISymbol symbol,
                                                      SourceProductionContext context,
                                                      string typeName,
                                                      MappingType mappingType,
                                                      IEnumerable<Type> allowedTypes) =>
            symbol.SetDiagnostic(context, DiagnosticsDescriptors.InvalidType, typeName, mappingType, string.Join(", ", allowedTypes.Select(t => t.Name)));
        internal static void SetInvalidTypeDiagnostic(this ISymbol symbol,
                                                      SourceProductionContext context,
                                                      string typeName,
                                                      MappingType mappingType,
                                                      IEnumerable<string> allowedTypesNames) =>
            symbol.SetDiagnostic(context, DiagnosticsDescriptors.InvalidType, typeName, mappingType, string.Join(", ", allowedTypesNames.Select(tn => tn)));

        internal static INamedTypeSymbol GetUnelyingType(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
                throw new Exception("[GetUnelyingType] expected INamedTypeSymbol");
            if (namedTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                if (namedTypeSymbol.OriginalDefinition.Name == "Nullable")
                    return (INamedTypeSymbol)namedTypeSymbol.TypeArguments.First();
                return namedTypeSymbol.OriginalDefinition;
            }
            return namedTypeSymbol;
        }
        internal static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol) =>
            symbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
        internal static IEnumerable<AttributeData> GetAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol) =>
            symbol.GetAttributes().Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));

    }
}
