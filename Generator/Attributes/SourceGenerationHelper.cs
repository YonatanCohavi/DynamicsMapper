using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamicsMapper.Attributes
{
    internal class SourceGenerationHelper
    {
        public static ITypeSymbol GetTypeSymbol(ITypeSymbol typeSymbol, out bool nullable)
        {
            ITypeSymbol type;
            nullable = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
            if (nullable && typeSymbol is INamedTypeSymbol namedType)
                type = namedType.TypeArguments.FirstOrDefault() ?? typeSymbol;
            else
                type = typeSymbol;
            return type;
        }
        public static string GenerateFieldTypeEnum()
        {
            var writer = new CodeWriter();
            using (writer.BeginScope("namespace Generator.Attributes"))
            {
                using (writer.BeginScope("public enum FieldType"))
                {
                    writer.AppendLine("Regular = 1,");
                    writer.AppendLine("Lookup = 2,");
                }
            }
            return writer.ToString();
        }
        public static string GenerateCrmEntityAttribute()
        {
            var writer = new CodeWriter();
            using (writer.BeginScope("namespace Generator.Attributes"))
            {
                writer.AppendLine("[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]");
                using (writer.BeginScope("public class CrmEntityAttribute : Attribute"))
                {
                    writer.AppendLine("public string LogicalName { get; set; }");
                    using (writer.BeginScope("public CrmEntityAttribute(string logicalname)"))
                    {
                        writer.AppendLine("LogicalName = logicalname;");
                    }
                }
            }
            return writer.ToString();
        }
        public static string GenerateCrmReferenceAttribute()
        {
            var writer = new CodeWriter();
            using (writer.BeginScope("namespace Generator.Attributes"))
            {
                using (writer.BeginScope("public class CrmReferenceAttribute : CrmFieldAttribute"))
                {
                    writer.AppendLine("public string Target { get; }");
                    using (writer.BeginScope("public CrmReferenceAttribute(string name, string target) : base(name)"))
                    {
                        writer.AppendLine("Target = target;");
                    }
                }
            }
            return writer.ToString();
        }
        public static string GenerateCrmFieldAttribute()
        {
            var writer = new CodeWriter();
            using (writer.BeginScope("namespace Generator.Attributes"))
            {
                writer.AppendLine("[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]");
                using (writer.BeginScope("public class CrmFieldAttribute : Attribute"))
                {
                    writer.AppendLine("public string Name { get; set; }");
                    using (writer.BeginScope("public CrmFieldAttribute(string name)"))
                    {
                        writer.AppendLine("Name = name;");
                    }
                }
            }
            return writer.ToString();
        }
        public static string GenerateCrmFormattedAttribute()
        {
            var writer = new CodeWriter();
            using (writer.BeginScope("namespace Generator.Attributes"))
            {
                writer.AppendLine("[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]");
                using (writer.BeginScope("public class CrmFormattedAttribute : Attribute"))
                {
                    writer.AppendLine("public string Name { get; set; }");
                    using (writer.BeginScope("public CrmFormattedAttribute(string name)"))
                    {
                        writer.AppendLine("Name = name;");
                    }
                }
            }
            return writer.ToString();
        }
        public static string GenerateCrmMoneyAttribute()
        {
            var writer = new CodeWriter();
            using (writer.BeginScope("namespace Generator.Attributes"))
            {
                writer.AppendLine("[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]");
                using (writer.BeginScope("public class CrmMoneyAttribute : Attribute"))
                {
                    writer.AppendLine("public string Name { get; set; }");
                    using (writer.BeginScope("public CrmMoneyAttribute(string name)"))
                    {
                        writer.AppendLine("Name = name;");
                    }
                }
            }
            return writer.ToString();
        }
        public static string GenerateCrmOptionsAttribute()
        {
            var writer = new CodeWriter();
            using (writer.BeginScope("namespace Generator.Attributes"))
            {
                writer.AppendLine("[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]");
                using (writer.BeginScope("public class CrmOptionsAttribute : Attribute"))
                {
                    writer.AppendLine("public string Name { get; set; }");
                    using (writer.BeginScope("public CrmOptionsAttribute(string name)"))
                    {
                        writer.AppendLine("Name = name;");
                    }
                }
            }
            return writer.ToString();
        }

    }
}
