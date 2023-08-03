using DynamicsMapper.Helpers;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicsMapper.Extentions
{
    public static class SyntaxNodeExtensions
    {
        public static bool InheritsFromDecoratedClass(this INamedTypeSymbol symbol, INamedTypeSymbol attributeSymbol)
        {
            if (symbol.BaseType != null)
            {
                if (symbol.BaseType.HasAttribute(attributeSymbol))
                    return true;
                return symbol.BaseType.InheritsFromDecoratedClass(attributeSymbol);
            }
            return false;
        }
        public static IEnumerable<IPropertySymbol> GetAllProperties(this INamedTypeSymbol symbol)
        {
            var properties = new List<IPropertySymbol>();   
            properties.AddRange(symbol.GetMembers().OfType<IPropertySymbol>());
            if (symbol.BaseType != null)
                properties.AddRange(symbol.BaseType.GetMembers().OfType<IPropertySymbol>());

            return properties;
        }
        public static T? GetParent<T>(this SyntaxNode node) where T : SyntaxNode
        {
            var parent = node.Parent;
            while (parent is not null)
            {
                if (parent is T type)
                    return type;
                parent = parent.Parent;
            }
            return null;
        }
    }
}
