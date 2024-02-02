using DebuggerClient.Enums;
using Microsoft.Xrm.Sdk;
using Tests.Entities;
using Tests.Enums;

namespace Tests
{
    [TestClass]
    public class MapperTests
    {
        private static readonly Guid _contactId = Guid.NewGuid();
        private static readonly Guid _accountId = Guid.NewGuid();
        private static readonly Color[] _colors = [Color.Red, Color.BlueYellow, Color.Green];
        private static readonly int[] _colorsInt = _colors.Cast<int>().ToArray();

        private static readonly EntityReference _accountRef = new("account", _accountId) { Name = "Parent Account" };
        private static readonly Contact _sourceModel = new()
        {
            ContactId = _contactId,
            Firstname = "john",
            Age = 15,
            Birthdate = new DateTime(2020, 1, 1),
            ContactType = ContactType.Member,
            IntContantType = (int)ContactType.Partner,
            Sallery = 1500m,
            FavoriteColors = _colors,
            FavoriteColorsInt = _colorsInt,
            AccountId = _accountId,
        };

        private static readonly Entity _sourceEntity = new("contact")
        {
            ["contactid"] = _contactId,
            ["rtm_i_age"] = 15,
            ["rtm_s_firstname"] = "john",
            ["rtm_o_type_int"] = new OptionSetValue((int)ContactType.Partner),
            ["rtm_o_type"] = new OptionSetValue((int)ContactType.Member),
            ["rtm_dt_birthdate"] = new DateTime(2020, 1, 1),
            ["rtm_l_account"] = _accountRef,
            ["new_sallery"] = new Money(1500),
            ["rtm_mo_fav_colors"] = new OptionSetValueCollection(_colors.Select(c => new OptionSetValue((int)c)).ToList()),
            ["rtm_mo_fav_colors_int"] = new OptionSetValueCollection(_colorsInt.Select(c => new OptionSetValue(c)).ToList()),
        };
        private static Entity MapModel()
        {
            var mapper = new ContactMapper();
            var entity = mapper.Map(_sourceModel);
            return entity;

        }
        public static void ModelToEntityAsserts(Entity entity)
        {
            Assert.AreEqual(_contactId, entity.Id);
            Assert.AreEqual((int)ContactType.Partner, entity.GetAttributeValue<OptionSetValue>("rtm_o_type_int").Value);
            Assert.AreEqual("john", entity.GetAttributeValue<string>("rtm_s_firstname"));
            Assert.AreEqual(ContactType.Member, (ContactType)entity.GetAttributeValue<OptionSetValue>("rtm_o_type").Value);
            Assert.AreEqual(15, entity["rtm_i_age"]);
            Assert.AreEqual(new DateTime(2020, 1, 1), entity.GetAttributeValue<DateTime>("rtm_dt_birthdate"));
            Assert.AreEqual(_accountRef, entity.GetAttributeValue<EntityReference>("rtm_l_account"));
            Assert.AreEqual(1500, entity.GetAttributeValue<Money>("new_sallery").Value);
            CollectionAssert.AreEqual(_colors, entity.GetAttributeValue<OptionSetValueCollection>("rtm_mo_fav_colors").Select(v => (Color)v.Value).ToArray());
            CollectionAssert.AreEqual(_colorsInt, entity.GetAttributeValue<OptionSetValueCollection>("rtm_mo_fav_colors_int").Select(v => v.Value).ToArray());
        }
        [TestMethod]
        public void EntityToModel()
        {
            var mapper = new ContactMapper();
            _sourceEntity.FormattedValues.Add("rtm_l_account", _accountRef.Name);
            var model = mapper.Map(_sourceEntity);
            Assert.AreEqual(_contactId, model.ContactId);
            Assert.AreEqual((int)ContactType.Partner, model.IntContantType);
            Assert.AreEqual("john", model.Firstname);
            Assert.AreEqual(ContactType.Member, model.ContactType);
            Assert.AreEqual(15, model.Age);
            Assert.AreEqual(new DateTime(2020, 1, 1), model.Birthdate);
            Assert.AreEqual(_accountRef.Id, model.AccountId);
            Assert.AreEqual(1500, model.Sallery);
            Assert.AreEqual("Parent Account", model.AccountName);
            CollectionAssert.AreEqual(_colors, model.FavoriteColors);
            CollectionAssert.AreEqual(_colorsInt, model.FavoriteColorsInt);
        }

        [TestMethod]
        public void ModelToEntity()
        {
            var entity = MapModel();
            ModelToEntityAsserts(entity);
        }

        //[TestMethod]
        //public void ModelToEntitySkipDefault()
        //{
        //    var settings = new MapperSettings
        //    {
        //        SkipDefaultValues = true
        //    };
        //    var entity = MapModel(settings);
        //    ModelToEntityAsserts(entity);
        //    Assert.AreEqual(8, entity.Attributes.Count);
        //}
    }
}