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
        public string? SchemaName { get; private set; }
        public IPropertySymbol PropertySymbol { get; private set; }
        public string? Target { get; private set; }
        public string? Alias { get; private set; }

        private FieldGenerationDetails(IPropertySymbol propertySymbol, MappingType mapping)
        {
            Mapping = mapping;
            PropertySymbol = propertySymbol;
        }
        public static FieldGenerationDetails CreateAlias(IPropertySymbol propertySymbol, string alias)
        {
            return new FieldGenerationDetails(propertySymbol, MappingType.Link)
            {
                Alias = alias
            };
        }
        public FieldGenerationDetails(string logicalName, IPropertySymbol propertySymbol, MappingType mapping = MappingType.Basic, string? target = null)
            :this(propertySymbol,mapping)
        {
            SchemaName = logicalName;
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
