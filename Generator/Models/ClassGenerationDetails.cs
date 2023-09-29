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
        public MappingType Mapping { get; private set; }
        public string SchemaName { get; private set; }
        public IPropertySymbol PropertySymbol { get; private set; }
        public string? Target { get; private set; }
        public string? Alias { get; private set; }

        private FieldGenerationDetails() { }
        public static FieldGenerationDetails CreateAlias(IPropertySymbol propertySymbol, string alias)
        {
            return new FieldGenerationDetails
            {
                Mapping = MappingType.Link,
                PropertySymbol = propertySymbol,
                Alias = alias
            };
        }
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
