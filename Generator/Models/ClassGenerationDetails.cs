using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Generator.Models
{
    public enum FieldType
    {
        Regular = 1,
        Lookup = 2,
        Money = 3,
        Formatted = 4,
        Options = 5,
    }
    internal class FieldGenerationDetails
    {
        public FieldType FieldType { get; set; } = FieldType.Regular;
        public string SchemaName { get; set; }
        public string? Target { get; set; }
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public bool Nullable { get; set; }

        public FieldGenerationDetails(string logicalName, string propertyName, string propertyType, bool nullable)
        {
            SchemaName = logicalName;
            PropertyName = propertyName;
            PropertyType = propertyType;
            Nullable = nullable;
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
