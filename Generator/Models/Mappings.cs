using DynamicsMapper.Abstractions;

namespace DynamicsMapper.Models
{
    internal record struct Mappings(string ToModel, string ToEntity, MappingType Type);
}
