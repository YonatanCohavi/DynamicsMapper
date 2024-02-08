using DynamicsMapper.Abstractions;

namespace DebuggerClient.Models
{
    [CrmEntity("email")]
    public class Email
    {
        [CrmField("activityid", Mapping = MappingType.PrimaryId)]
        public Guid? Id { get; set; }

        [CrmField("subject")]
        public string? Subject { get; set; }
        [CrmField("regardingobjectid", Mapping = MappingType.DynamicLookup)]
        public Guid? Regarding { get; set; }

    }
}
