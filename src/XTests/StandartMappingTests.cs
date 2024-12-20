using DynamicsMapper.Abstractions;
using DynamicsMapper.Abstractions.Settings;
using Microsoft.Xrm.Sdk;
using XTests.Models;

namespace XTests;
public class StandartMappingTests
{
    private readonly DataFactory dataFactory = new();

    [Fact]
    public void Entity_To_Model()
    {
        var sourceEntity = dataFactory.GetContactEntity();
        var sourceModel = dataFactory.GetContactModel();
        var mapper = new ContactMapper();
        var entity = sourceEntity;
        var model = mapper.Map(entity);

        Assert.NotNull(model);
        Assert.Equal(sourceModel.Id, model.Id);
        Assert.Equal(sourceModel.IntContantType, model.IntContantType);
        Assert.Equal(sourceModel.Firstname, model.Firstname);
        Assert.Equal(sourceModel.ContactType, model.ContactType);
        Assert.Equal(sourceModel.Age, model.Age);
        Assert.Equal(sourceModel.Birthdate, model.Birthdate);
        Assert.Equal(sourceModel.AccountId, model.AccountId);
        Assert.Equal(sourceModel.Sallery, model.Sallery);
        Assert.Equal(sourceModel.AccountName, model.AccountName);
        Assert.Equal(sourceModel.FavoriteColors, model.FavoriteColors);
        Assert.Equal(sourceModel.FavoriteColorsInt, model.FavoriteColorsInt);
    }
    [Fact]
    public void Model_To_Entity()
    {
        var sourceEntity = dataFactory.GetContactEntity();
        var sourceModel = dataFactory.GetContactModel();
        var mapper = new ContactMapper();
        var entity = mapper.Map(sourceModel);

        Assert.NotNull(entity);
        Assert.Equal(entity.Id, sourceEntity.Id);
        Assert.Equal(entity.GetAttributeValue<OptionSetValue>("rtm_o_type_int"), sourceEntity.GetAttributeValue<OptionSetValue>("rtm_o_type_int"));
        Assert.Equal(entity.GetAttributeValue<string>("rtm_s_firstname"), sourceEntity.GetAttributeValue<string>("rtm_s_firstname"));
        Assert.Equal(entity.GetAttributeValue<OptionSetValue>("rtm_o_type"), sourceEntity.GetAttributeValue<OptionSetValue>("rtm_o_type"));
        Assert.Equal(entity.GetAttributeValue<int>("rtm_i_age"), sourceEntity.GetAttributeValue<int>("rtm_i_age"));
        Assert.Equal(entity.GetAttributeValue<DateTime>("rtm_dt_birthdate"), sourceEntity.GetAttributeValue<DateTime>("rtm_dt_birthdate"));
        Assert.Equal(entity.GetAttributeValue<EntityReference>("rtm_l_account"), sourceEntity.GetAttributeValue<EntityReference>("rtm_l_account"));
        Assert.Equal(entity.GetAttributeValue<Money>("new_sallery"), sourceEntity.GetAttributeValue<Money>("new_sallery"));
        Assert.Equal(entity.GetAttributeValue<Money>("new_sallery"), sourceEntity.GetAttributeValue<Money>("new_sallery"));
        Assert.Equal(entity.GetAttributeValue<OptionSetValueCollection>("rtm_mo_fav_colors").Count, sourceEntity.GetAttributeValue<OptionSetValueCollection>("rtm_mo_fav_colors").Count);

        var mappedFavValues = entity.GetAttributeValue<OptionSetValueCollection>("rtm_mo_fav_colors");
        var sourceFavValues = sourceEntity.GetAttributeValue<OptionSetValueCollection>("rtm_mo_fav_colors");
        Assert.Equal(mappedFavValues, sourceFavValues);
        Assert.Equal(mappedFavValues.Intersect(sourceFavValues).Count(), sourceFavValues.Count);

        var mappedFavValuesInt = entity.GetAttributeValue<OptionSetValueCollection>("rtm_mo_fav_colors_int");
        var sourceFavValuesInt = sourceEntity.GetAttributeValue<OptionSetValueCollection>("rtm_mo_fav_colors_int");
        Assert.Equal(mappedFavValuesInt, sourceFavValuesInt);
        Assert.Equal(mappedFavValuesInt.Intersect(sourceFavValuesInt).Count(), sourceFavValuesInt.Count);
    }

    [Fact]
    public void Model_To_Entity_Skip_Default()
    {
        var sourceEntity = dataFactory.GetContactEntity();
        var sourceModel = dataFactory.GetContactModel();
        var settings = new DynamicsMapperSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };
        var mapper = new ContactMapper();
        var entity = mapper.Map(sourceModel, settings);
        Assert.Equal(10, entity.Attributes.Count);
    }
}
