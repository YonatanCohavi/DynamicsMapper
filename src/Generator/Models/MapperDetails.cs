using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicsMapper.Models
{
    internal class MapperDetails
    {
        public MapperDetails(ClassDeclarationSyntax classDeclarationSyntax, INamedTypeSymbol mapperSymbol, string mapperClassName, string entityName)
        {
            ClassDeclarationSyntax = classDeclarationSyntax;
            MapperSymbol = mapperSymbol;
            MapperClassName = mapperClassName;
            EntityName = entityName;
        }

        public ClassDeclarationSyntax ClassDeclarationSyntax { get; set; }
        public INamedTypeSymbol MapperSymbol { get; set; }
        public string MapperClassName { get; set; }
        public string EntityName { get; set; }
    }
}
