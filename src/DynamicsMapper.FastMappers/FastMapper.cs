using DynamicsMapper.Abstractions;
using Microsoft.Xrm.Sdk;
using System.Linq.Expressions;

namespace DynamicsMapper.FastMappers;
public class FastMapper<TModel, TPModel>(Expression<Func<TModel, TPModel>> selector)
{
    private readonly FastModelMapper<TPModel> fastModelMapper = MapperFactory.ToModelMapper(selector);
    private readonly FastEntityMapper<TPModel> fastEntityMapper = MapperFactory.ToEntityMapper(selector);
    public TPModel Map(Entity e) => fastModelMapper.Map(e);
    public TPModel Map(Entity e, DynamicsMapperSettings settings) => fastModelMapper.Map(e, settings);
    public Entity Map(TPModel model) => fastEntityMapper.Map(model);
    public Entity Map(TPModel model, DynamicsMapperSettings settings) => fastEntityMapper.Map(model, settings);
}