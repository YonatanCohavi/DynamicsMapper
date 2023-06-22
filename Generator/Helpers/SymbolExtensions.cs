using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicsMapper.Helpers
{
    internal static class SymbolExtensions
    {
        internal static bool HasAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol) =>
            symbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
        internal static IEnumerable<AttributeData> GetAttribute(this ISymbol symbol, INamedTypeSymbol attributeSymbol) =>
            symbol.GetAttributes().Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));

    }
}
