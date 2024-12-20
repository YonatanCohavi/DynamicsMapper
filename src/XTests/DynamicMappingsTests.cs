using Microsoft.Xrm.Sdk;
using XTests.Models;

namespace XTests;

public class DynamicMappingsTests
{
    [Fact]
    public void Targets_To_Entity()
    {
        var emailId = Guid.NewGuid();
        var regardingId = Guid.NewGuid();
        var email = new Email
        {
            Regarding = regardingId,
            Id = emailId,
            Subject = "email subject"
        };
        var mapper = new EmailMapper();
        var targets = new EmailTargets { Regarding = "account", OwnerId = "team", };
        var failingTargets = new EmailTargets { Regarding = string.Empty };
        var entity = mapper.Map(email, targets);
        var regardingRef = entity.GetAttributeValue<EntityReference>("regardingobjectid");
        var ownerRef = entity.GetAttributeValue<EntityReference>("ownerid");
        Assert.Throws<ArgumentException>(() => mapper.Map(email, failingTargets));
        Assert.Equal("account", regardingRef.LogicalName);
        Assert.Equal(regardingId, regardingRef.Id);
        Assert.Null(ownerRef);
    }
    [Fact]
    public void Dynamic_Lookup_Target_Mapping_Type_To_Entity()
    {
        var emailId = Guid.NewGuid();
        var regardingId = Guid.NewGuid();
        var email = new Email
        {
            Regarding = regardingId,
            RegardingTarget = "account",
            Id = emailId,
            Subject = "email subject",
        };

        var mapper = new EmailMapper();
        var targets = new EmailTargets { OwnerId = "systemuser" };
        var entity = mapper.Map(email, targets);
        var regardingRef = entity.GetAttributeValue<EntityReference>("regardingobjectid");
        var ownerRef = entity.GetAttributeValue<EntityReference>("ownerid");
        Assert.Equal("account", regardingRef.LogicalName);
        Assert.Equal(regardingId, regardingRef.Id);
        Assert.Null(ownerRef);
    }
    [Fact]
    public void Dynamic_Lookup_Target_Mapping_Type_To_Model()
    {
        var emailId = Guid.NewGuid();
        var regardingId = Guid.NewGuid();
        var entity = new Entity("email", emailId)
        {
            ["regardingobjectid"] = new EntityReference("account", regardingId),
            ["subject"] = "email subject"
        };

        var mapper = new EmailMapper();
        var email = mapper.Map(entity);
        Assert.Equal("account", email.RegardingTarget);
        Assert.Equal(regardingId, email.Regarding);
        Assert.Null(email.OwnerId);
    }
}