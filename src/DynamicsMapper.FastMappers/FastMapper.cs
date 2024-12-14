using DynamicsMapper.Abstractions;
using Microsoft.Xrm.Sdk;
using System.Linq.Expressions;

namespace DynamicsMapper.FastMappers;
public class FastMapper<TModel, TPModel>(Expression<Func<TModel, TPModel>> selector)
{
    private readonly Lazy<FastModelMapper<TPModel>> fastModelMapper = new Lazy<FastModelMapper<TPModel>>(() => MapperFactory.ToModelMapper(selector));
    private readonly Lazy<FastEntityMapper<TPModel>> fastEntityMapper = new Lazy<FastEntityMapper<TPModel>>(() => MapperFactory.ToEntityMapper(selector));
    public TPModel Map(Entity e) => fastModelMapper.Value.Map(e);
    public TPModel Map(Entity e, DynamicsMapperSettings settings) => fastModelMapper.Value.Map(e, settings);
    public Entity Map(TPModel model) => fastEntityMapper.Value.Map(model);
    public Entity Map(TPModel model, DynamicsMapperSettings settings) => fastEntityMapper.Value.Map(model, settings);
}