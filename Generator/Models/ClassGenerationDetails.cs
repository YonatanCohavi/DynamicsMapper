using DynamicsMapper.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicsMapper.Models
{

    internal class FieldGenerationDetails
    {
        public MappingType Mapping { get; }
        public string SchemaName { get; }
        public IPropertySymbol PropertySymbol { get; }
        public string? Target { get; }

        public FieldGenerationDetails(string logicalName, IPropertySymbol propertySymbol, MappingType mapping = MappingType.Basic, string? target = null)
        {
            SchemaName = logicalName;
            PropertySymbol = propertySymbol;
            Mapping = mapping;
            Target = target;
        }
    }
    internal class ClassGenerationDetails
    {
        public ClassGenerationDetails(string entityName, ClassDeclarationSyntax @class)
        {
            EntityName = entityName;
            Class = @class;
        }

        public string EntityName { get; }
        public ClassDeclarationSyntax Class { get; }
    }
}
