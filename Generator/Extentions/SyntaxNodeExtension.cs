using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Generator.Extensions
{
    public static class SyntaxNodeExtensions
    {
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
