 using System;

namespace DynamicsMapper.Abstractions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CrmFieldAttribute : Attribute
    {
        public string SchemaName { get; set; }
        public MappingType Mapping { get; set; }
        public string Target { get; set; }

        public CrmFieldAttribute(string schemaName)
        {
            SchemaName = schemaName;
        }
    }
}
