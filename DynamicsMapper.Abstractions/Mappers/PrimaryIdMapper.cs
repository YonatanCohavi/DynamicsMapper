using Microsoft.Xrm.Sdk;
using System;

namespace DynamicsMapper.Abstractions.Mappers
{
    public class PrimaryIdMapper : IPrimaryIdMapper
    {
        private static PrimaryIdMapper? _instance;
        public static PrimaryIdMapper Instance => _instance ??= new();
        private PrimaryIdMapper() { }
        public Guid? MapToEntity(Guid? id) => id;
        public Guid? MapToModel(Entity entity, string attribute)
        {
            if (entity.Id != Guid.Empty)
                return entity.Id;

            return entity.GetAttributeValue<Guid?>(attribute);
        }
    }

    public interface IPrimaryIdMapper
    {
        Guid? MapToEntity(Guid? id);
        Guid? MapToModel(Entity entity, string attribute);
    }
}
