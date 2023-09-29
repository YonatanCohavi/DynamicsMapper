using System;

namespace DynamicsMapper.Abstractions
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CrmLinkAttribute : Attribute
    {
        public string Alias { get; set; }

        public CrmLinkAttribute(string alias)
        {
            Alias = alias;
        }
    }
}
