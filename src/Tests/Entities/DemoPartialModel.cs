using System;

namespace Tests.Entities
{
    public class DemoPartialModel
    {
        public Guid Id { get; set; }
        public Guid? AnotherId { get; set; }
        public int? IntContantType { get; set; }
        public string? StaticValue { get; set; }
    }
}
