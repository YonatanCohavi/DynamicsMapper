using Microsoft.Xrm.Sdk;
using Tests.Entities;

namespace Tests
{
    [TestClass]
    public class FastMappersTests
    {
        [TestMethod]
        public void FastToModelMapper()
        {
            var fastMapper = DemoEntityMapper.CreatePartialMapper((e) => new { e.Age, e.Birthdate });

            var model = fastMapper.Map(TestData.SourceEntity);
            Assert.AreEqual(model.Age, TestData.SourceEntity.GetAttributeValue<int>("rtm_i_age"));
            Assert.AreEqual(model.Birthdate, TestData.SourceEntity.GetAttributeValue<DateTime>("rtm_dt_birthdate"));
        }

        [TestMethod]
        public void FastToModelMapper_ClassDestination()
        {
            var fastMapper = DemoEntityMapper.CreatePartialMapper((e) => new DemoPartialModel
            {
                Id = e.DemoId!.Value,
                IntContantType = e.IntContantType,
                StaticValue = "static Value"
            });
            var model = fastMapper.Map(TestData.SourceEntity);
            Assert.AreEqual(model.StaticValue, "static Value");
            Assert.AreEqual(model.Id, TestData.SourceEntity.Id);
            Assert.AreEqual(model.IntContantType, TestData.SourceEntity.GetAttributeValue<OptionSetValue>("rtm_o_type_int").Value);
        }

        [TestMethod]
        public void FastToEntityMapper()
        {
            var fastModelMapper = DemoEntityMapper.CreatePartialMapper((c) => new
            {
                c.DemoId,
            });
            var entity = fastModelMapper.Map(new { DemoId = (Guid?)TestData.EntityId });
            Assert.AreEqual(entity.Id, TestData.EntityId);
            Assert.AreEqual(entity.GetAttributeValue<Guid>("demoid"), TestData.EntityId);
        }

        [TestMethod]
        public void FastToEntityMappper()
        {
            var fullMapper = new DemoEntityMapper();
            var fastModelMapper = DemoEntityMapper.CreatePartialMapper((c) => new
            {
                c.DemoId,
                c.Firstname,
                c.Lastname,
                c.Age,
                c.Birthdate,
                c.ContactType,
                c.IntContantType,
                c.Sallery,
                c.FavoriteColors,
                c.FavoriteColorsInt,
                c.AccountId,
                c.RegardingId,
                RegardingIdTarget = "account",
            });
            var partialEntity = fastModelMapper.Map(new
            {
                TestData.SourceModel.DemoId,
                TestData.SourceModel.Firstname,
                TestData.SourceModel.Lastname,
                TestData.SourceModel.Age,
                TestData.SourceModel.Birthdate,
                TestData.SourceModel.ContactType,
                TestData.SourceModel.IntContantType,
                TestData.SourceModel.Sallery,
                TestData.SourceModel.FavoriteColors,
                TestData.SourceModel.FavoriteColorsInt,
                TestData.SourceModel.AccountId,
                TestData.SourceModel.RegardingId,
                RegardingIdTarget = "account",
            });

            Assert.AreEqual(partialEntity.Id, TestData.SourceEntity.Id);
            foreach (var attribute in partialEntity.Attributes.Keys)
            {
                if (attribute == "rtm_mo_fav_colors" || attribute == "rtm_mo_fav_colors_int")
                {
                    CollectionAssert.AreEqual(partialEntity.GetAttributeValue<OptionSetValueCollection>(attribute), TestData.SourceEntity.GetAttributeValue<OptionSetValueCollection>(attribute));

                    continue;
                }
                Assert.AreEqual(partialEntity[attribute], TestData.SourceEntity[attribute]);
            }
        }
    }
}
