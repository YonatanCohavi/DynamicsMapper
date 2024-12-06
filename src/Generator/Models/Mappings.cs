using DynamicsMapper.Abstractions;

namespace DynamicsMapper.Models
{
    internal record struct Mappings(string ToModel, string ToEntity, string AttributeType, string AttributRef, MappingType Type, FieldGenerationDetails Attribute);
}
