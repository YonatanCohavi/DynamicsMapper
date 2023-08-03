using DynamicsMapper.Abstractions;

namespace DebuggerClient.Models
{
    [CrmEntity("city")]
    public partial class CityBase
    {
        [CrmField("cityid", Mapping = MappingType.PrimaryId)]
        public Guid CityId { get; set; }
        [CrmField("name")]
        public string? BaseName { get; set; }
    }

    [CrmEntity("city")]
    public partial class FullCity : CityBase
    {
    }

}
