using DynamicsMapper.Abstractions;
using DynamicsMapper.Extension;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

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

        public Account? Map(Entity entity, string alias) => InternalMap(entity, alias);
        public Account Map(Entity entity) => InternalMap(entity)!;
        private static Account? InternalMap(Entity source, string? alias = null)
        {
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
                throw new ArgumentException($"entity LogicalName expected to be {entityname} recived: {entity?.LogicalName}", nameof(source));
            var account = new Account();
            account.AccountId = entity.GetAttributeValue<Guid>("accountid");
            var contactMapper = new DebuggerClient.Models.ContactMapper();
            var mapped_contact = contactMapper.Map(source, "contact");
            if (mapped_contact != null)
                account.Contact = mapped_contact;
            return account;
        }
    }
}
