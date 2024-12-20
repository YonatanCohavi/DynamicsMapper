using Microsoft.Xrm.Sdk;
using XTests.Models;

namespace XTests;
public class DataFactory
{
    private readonly Guid _contactId = Guid.NewGuid();
    private readonly Guid _regardingId = Guid.NewGuid();
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly Color[] colors = [Color.Red, Color.BlueYellow, Color.Green];
    private readonly int[] colorsInt = new[] { Color.Red, Color.Green }.Cast<int>().ToArray();
    private string _regardiingName = "regarding";

    public Contact GetContactModel() => new()
    {
        Id = _contactId,
        Firstname = "john",
        Lastname = null,
        Age = 15,
        Birthdate = new DateTime(2020, 1, 1),
        ContactType = ContactType.Member,
        IntContantType = (int)ContactType.Partner,
        Sallery = 1500m,
        FavoriteColors = colors,
        FavoriteColorsInt = colorsInt,
        AccountId = _accountId,
        RegardingId = _regardingId,
        RegardingName = _regardiingName
    };
    public Entity GetContactEntity()
    {
        var model = GetContactModel();
        var entity = new Entity("contact", model.Id!.Value)
        {
            ["contactid"] = model.Id,
            ["regardingobjectid"] = new EntityReference("account", model.RegardingId!.Value),
            ["rtm_i_age"] = model.Age,
            ["rtm_s_lastname"] = model.Lastname,
            ["rtm_s_firstname"] = model.Firstname,
            ["rtm_o_type_int"] = model.IntContantType.HasValue ? new OptionSetValue(model.IntContantType.Value) : null,
            ["rtm_o_type"] = model.ContactType.HasValue ? new OptionSetValue((int)model.ContactType.Value) : null,
            ["rtm_dt_birthdate"] = model.Birthdate,
            ["rtm_l_account"] = model.AccountId.HasValue ? new EntityReference("account", model.AccountId.Value) : null,
            ["new_sallery"] = model.Sallery.HasValue ? new Money(model.Sallery.Value) : null,
            ["rtm_mo_fav_colors"] = new OptionSetValueCollection(model.FavoriteColors!.Select(c => new OptionSetValue((int)c)).ToList()),
            ["rtm_mo_fav_colors_int"] = new OptionSetValueCollection(model.FavoriteColorsInt!.Select(c => new OptionSetValue(c)).ToList()),
        };
        entity.FormattedValues.Add("regardingobjectid", _regardiingName);
        return entity;
    }
}
