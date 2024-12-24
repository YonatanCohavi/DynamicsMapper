using XTests.Models;

namespace XTests;
public class PartialMappersTests
{
    [Fact]
    public void PartialMapper()
    {
        var leadId = Guid.NewGuid();
        var partialLeadMapper = LeadMapper.CreatePartialMapper(l => new { Id = l.Id!.Value, Age = l.Age!.Value });
        var leadToUpdate = partialLeadMapper.Map(new { Id = leadId, Age = 30 });

        Assert.Equal(30, leadToUpdate.GetAttributeValue<int>("new_age"));
        Assert.Equal(leadId, leadToUpdate.GetAttributeValue<Guid>("leadid"));
        Assert.Equal(leadId, leadToUpdate.Id);
    }
}
