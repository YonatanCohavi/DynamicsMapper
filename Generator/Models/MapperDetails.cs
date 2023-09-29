using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicsMapper.Models
{
    internal class MapperDetails
    {
        public ClassDeclarationSyntax ClassDeclarationSyntax { get; set; }
        public INamedTypeSymbol MapperSymbol { get; set; }
        public string MapperClassName { get; set; }
        public string EntityName { get; set; }
    }
}
