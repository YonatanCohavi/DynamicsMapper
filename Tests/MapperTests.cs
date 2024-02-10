using DebuggerClient.Enums;
using DebuggerClient.Models;
using DynamicsMapper.Abstractions;
using DynamicsMapper.Abstractions.Settings;
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
            Lastname = null,
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
        private static Entity MapModel(DynamicsMapperSettings settings)
        {
            var mapper = new ContactMapper();
            var entity = mapper.Map(_sourceModel, settings: settings);
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
        public void Dynamic_Mappings_To_Entity()
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
            var faiingTargets = new EmailTargets { Regarding = string.Empty };
            var entity = mapper.Map(email, targets);
            var regardingRef = entity.GetAttributeValue<EntityReference>("regardingobjectid");
            var ownerRef = entity.GetAttributeValue<EntityReference>("ownerid");
            Assert.ThrowsException<ArgumentException>(() => mapper.Map(email, faiingTargets));
            Assert.AreEqual("account", regardingRef.LogicalName);
            Assert.AreEqual(regardingId, regardingRef.Id);
            Assert.IsNull(ownerRef);
        }
        [TestMethod]
        public void Dynamic_Mappings_To_Entity_No_Targets()
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
            Assert.AreEqual("account", regardingRef.LogicalName);
            Assert.AreEqual(regardingId, regardingRef.Id);
            Assert.IsNull(ownerRef);
        }
        [TestMethod]
        public void Dynamic_Mappings_To_Model()
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
            Assert.AreEqual("account", email.RegardingTarget);
            Assert.AreEqual(regardingId, email.Regarding);
            Assert.IsNull(email.OwnerId);
        }
        [TestMethod]
        public void Entity_To_Model()
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
        public void Model_To_Entity()
        {
            var entity = MapModel(DynamicsMapperSettings.Default);
            ModelToEntityAsserts(entity);
            Assert.IsTrue(entity.Contains("rtm_s_lastname"));
            Assert.IsNull(entity["rtm_s_lastname"]);
        }

        [TestMethod]
        public void Model_To_Entity_Skip_Default()
        {
            var settings = new DynamicsMapperSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
            };
            var entity = MapModel(settings);
            ModelToEntityAsserts(entity);
            Assert.AreEqual(9, entity.Attributes.Count);
        }
    }
}