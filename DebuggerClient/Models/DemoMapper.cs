using DynamicsMapper.Abstractions;
using DynamicsMapper.Abstractions.Settings;
using DynamicsMapper.Extension;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DebuggerClient.Models
{
    public class EmailTargets1
    {
        public string Regarding { get; set; } = string.Empty;

        public static implicit operator DynamicsMappingsTargets(EmailTargets1 targets)
        {
            return new DynamicsMappingsTargets
            {
                {
                    "regardingobjectid",
                    targets.Regarding
                },
            };
        }
    }

    public class EmailMapper1 : IEntityMapper<Email>
    {
        private static readonly string[] columns = new[]
        {
            "activityid",
            "subject",
            "regardingobjectid"
        };
        public ColumnSet Columns => new ColumnSet(columns);

        private const string entityname = "email";
        public string Entityname => entityname;

        /// <summary> You can use <see cref = "EmailTargets"/> instead of <see cref = "DynamicsMappingsTargets"/></summary>
        public Entity Map(Email email, DynamicsMappingsTargets? dynamicMappingsTargets, DynamicsMapperSettings? settings = null)
        {
            settings ??= DynamicsMapperSettings.Default;
            var mappers = settings.Mappers;
            var entity = new Entity(entityname);
            entity.Id = mappers.PrimaryIdMapper.MapToEntity(email.Id) ?? Guid.Empty;
            if (settings.NullHandling != NullHandling.Skip && email.Subject != default)
                entity["subject"] = mappers.BasicMapper.MapToEntity<string?>(email.Subject);
            if (dynamicMappingsTargets?.TryGetValue("regardingobjectid", out var regardingobjectid_target) != true || string.IsNullOrEmpty(regardingobjectid_target))
                throw new ArgumentException("target not found for 'regardingobjectid'", nameof(dynamicMappingsTargets));
            entity["regardingobjectid"] = mappers.LookupMapper.MapToEntity(email.Regarding, regardingobjectid_target);
            return entity;
        }

        /// <summary> You can use <see cref = "EmailTargets"/> instead of <see cref = "DynamicsMappingsTargets"/></summary>
        public Email? Map(Entity entity, string alias, DynamicsMapperSettings? settings = null) => InternalMap(entity, settings, alias);
        /// <summary> You can use <see cref = "EmailTargets"/> instead of <see cref = "DynamicsMappingsTargets"/></summary>
        public Email Map(Entity entity, DynamicsMapperSettings? settings = null)
        {
            var email = InternalMap(entity, settings) ?? throw new Exception("Mapping failed");
            return email;
        }

        private Email? InternalMap(Entity source, DynamicsMapperSettings? settings, string? alias = null)
        {
            settings ??= DynamicsMapperSettings.Default;
            var mappers = settings.Mappers;
            Entity? entity;
            if (string.IsNullOrEmpty(alias))
            {
                entity = source;
            }
            else
            {
                entity = source.GetAliasedEntity(alias);
                if (entity is null)
                    return null;
            }

            if (entity?.LogicalName != entityname)
                throw new ArgumentException($"entity LogicalName expected to be {entityname} recived: {entity?.LogicalName}", "entity");
            var email = new Email();
            email.Id = mappers.PrimaryIdMapper.MapToModel(entity, "activityid");
            email.Subject = mappers.BasicMapper.MapToModel<string?>(entity, "subject");
            email.Regarding = mappers.LookupMapper.MapToModel(entity, "regardingobjectid");
            email.RegardingTarget = mappers.DynamicLookupTargetMapper.MapToModel(entity, "regardingobjectid");
            return email;
        }
    }
}