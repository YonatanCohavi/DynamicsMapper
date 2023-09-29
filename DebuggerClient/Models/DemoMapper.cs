using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using DynamicsMapper.Extension;
using DynamicsMapper.Mappers;
using System;

namespace DebuggerClient.Models
{
    public class AccountMapper2 : IEntityMapper<Account>
    {
        private static readonly string[] columns = new[]
        {
            "accountid"
        };
        public ColumnSet Columns => new ColumnSet(columns);

        private const string entityname = "account";
        public string Entityname => entityname;

        public Entity Map(Account account)
        {
            var entity = new Entity(entityname);
            entity.Id = account.AccountId;
            return entity;
        }

        public Account? Map(Entity source, string alias)
        { 
            var aliased = source.GetAliasedEntity(alias);
        }
        public Account Map(Entity source, string? alias = null)
        {
            var entity = string.IsNullOrEmpty(alias) ? source : source.GetAliasedEntity(alias);
            if (entity?.LogicalName != entityname)
                throw new ArgumentException($"entity LogicalName expected to be {entityname} recived: {entity?.LogicalName}", "entity");
            var account = new Account();
            account.AccountId = entity.GetAttributeValue<Guid>("accountid");
            var contactMapper = new DebuggerClient.Models.ContactMapper();
            var mapped_contact = contactMapper.Map(entity, "contact");
            if (mapped_contact != null)
                account.Contact = mapped_contact;
            return account;
        }
    }
}
}
