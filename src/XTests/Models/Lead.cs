using DynamicsMapper.Abstractions;

namespace XTests.Models;
[CrmEntity("lead")]
public class Lead
{
    [CrmField("leadid", Mapping = MappingType.PrimaryId)]
    public Guid? Id { get; set; }
    [CrmField("emailaddress1")]
    public string? Emailaddress1 { get; set; }
    [CrmField("firstname")]
    public string? Firstname { get; set; }
    [CrmField("lastname")]
    public string? Lastname { get; set; }
    [CrmField("new_age")]
    public int? Age { get; set; }
}