using Microsoft.Xrm.Sdk;
using Tests.Entities;
using Tests.Enums;

namespace Tests
{
    public static class TestData
    {
        public static readonly Guid EntityId = Guid.NewGuid();
        public static readonly Guid RegardingId = Guid.NewGuid();
        public static readonly Guid _accountId = Guid.NewGuid();
        public static readonly Color[] Colors = [Color.Red, Color.BlueYellow, Color.Green];
        public static readonly int[] ColorsInt = Colors.Cast<int>().ToArray();

        public static readonly EntityReference AccountRef = new("account", _accountId) { Name = "Parent Account" };
        public static readonly DemoEntity SourceModel = new()
        {
            DemoId = EntityId,
            Firstname = "john",
            Lastname = null,
            Age = 15,
            Birthdate = new DateTime(2020, 1, 1),
            ContactType = ContactType.Member,
            IntContantType = (int)ContactType.Partner,
            Sallery = 1500m,
            FavoriteColors = Colors,
            FavoriteColorsInt = ColorsInt,
            AccountId = _accountId,
            RegardingId = RegardingId,
        };

        public static readonly Entity SourceEntity = new("demo", EntityId)
        {
            ["demoid"] = SourceModel.DemoId,
            ["regardingobjectid"] = new EntityReference("account", SourceModel.RegardingId.Value),
            ["rtm_i_age"] = SourceModel.Age,
            ["rtm_s_lastname"] = SourceModel.Lastname,
            ["rtm_s_firstname"] = SourceModel.Firstname,
            ["rtm_o_type_int"] = SourceModel.IntContantType.HasValue ? new OptionSetValue(SourceModel.IntContantType.Value) : null,
            ["rtm_o_type"] = SourceModel.ContactType.HasValue ? new OptionSetValue((int)SourceModel.ContactType.Value) : null,
            ["rtm_dt_birthdate"] = SourceModel.Birthdate,
            ["rtm_l_account"] = SourceModel.AccountId.HasValue ? new EntityReference("account", SourceModel.AccountId.Value) : null,
            ["new_sallery"] = SourceModel.Sallery.HasValue ? new Money(SourceModel.Sallery.Value) : null,
            ["rtm_mo_fav_colors"] = new OptionSetValueCollection(SourceModel.FavoriteColors.Select(c => new OptionSetValue((int)c)).ToList()),
            ["rtm_mo_fav_colors_int"] = new OptionSetValueCollection(SourceModel.FavoriteColorsInt.Select(c => new OptionSetValue(c)).ToList()),
        };
    }
}
