using System;

namespace DynamicsMapper.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CrmEntityAttribute : Attribute
    {
        public string LogicalName { get; set; }
        public CrmEntityAttribute(string logicalname)
        {
            LogicalName = logicalname;
        }
    }
}
