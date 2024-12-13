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
        private static Entity MapModel(DynamicsMapperSettings settings)
        {
            var mapper = new DemoEntityMapper();
            return mapper.Map(TestData.SourceModel, settings: settings);
        }
        public static void ModelToEntityAsserts(Entity entity)
        {
            Assert.AreEqual(TestData.EntityId, entity.Id);
            Assert.AreEqual((int)ContactType.Partner, entity.GetAttributeValue<OptionSetValue>("rtm_o_type_int").Value);
            Assert.AreEqual("john", entity.GetAttributeValue<string>("rtm_s_firstname"));
            Assert.AreEqual(ContactType.Member, (ContactType)entity.GetAttributeValue<OptionSetValue>("rtm_o_type").Value);
            Assert.AreEqual(15, entity["rtm_i_age"]);
            Assert.AreEqual(new DateTime(2020, 1, 1), entity.GetAttributeValue<DateTime>("rtm_dt_birthdate"));
            Assert.AreEqual(TestData.AccountRef, entity.GetAttributeValue<EntityReference>("rtm_l_account"));
            Assert.AreEqual(1500, entity.GetAttributeValue<Money>("new_sallery").Value);
            CollectionAssert.AreEqual(TestData.Colors, entity.GetAttributeValue<OptionSetValueCollection>("rtm_mo_fav_colors").Select(v => (Color)v.Value).ToArray());
            CollectionAssert.AreEqual(TestData.ColorsInt, entity.GetAttributeValue<OptionSetValueCollection>("rtm_mo_fav_colors_int").Select(v => v.Value).ToArray());
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
            var mapper = new DemoEntityMapper();
            TestData.SourceEntity.FormattedValues.Add("rtm_l_account", TestData.AccountRef.Name);
            var model = mapper.Map(TestData.SourceEntity);
            Assert.AreEqual(TestData.EntityId, model.DemoId);
            Assert.AreEqual((int)ContactType.Partner, model.IntContantType);
            Assert.AreEqual("john", model.Firstname);
            Assert.AreEqual(ContactType.Member, model.ContactType);
            Assert.AreEqual(15, model.Age);
            Assert.AreEqual(new DateTime(2020, 1, 1), model.Birthdate);
            Assert.AreEqual(TestData.AccountRef.Id, model.AccountId);
            Assert.AreEqual(1500, model.Sallery);
            Assert.AreEqual("Parent Account", model.AccountName);
            CollectionAssert.AreEqual(TestData.Colors, model.FavoriteColors);
            CollectionAssert.AreEqual(TestData.ColorsInt, model.FavoriteColorsInt);
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
            Assert.AreEqual(10, entity.Attributes.Count);
        }
    }
}