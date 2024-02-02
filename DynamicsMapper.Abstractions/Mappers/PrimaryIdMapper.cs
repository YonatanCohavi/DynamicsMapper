using Microsoft.Xrm.Sdk;
using System;

namespace DynamicsMapper.Abstractions.Mappers
{
    public static class PrimaryIdMapper
    {
        public static Guid? MapToEntity(Guid? id) => id;
        public static Guid? MapToModel(Entity entity, string attribute)
        {
            if (entity.Id != Guid.Empty)
                return entity.Id;

            return entity.GetAttributeValue<Guid?>(attribute);
        }
    }
}
