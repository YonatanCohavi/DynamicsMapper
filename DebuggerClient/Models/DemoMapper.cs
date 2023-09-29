using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using DynamicsMapper.Extension;
using DynamicsMapper.Mappers;
using System;

namespace DebuggerClient.Models
{
    public class ContactMapper2 : IEntityMapper<Contact>
    {
        private static readonly string[] columns = new[]
        {
            "contactid",
            "rtm_o_test",
            "rtm_o_test2",
            "rtm_o_type",
            "rtm_o_type2",
            "rtm_s_firstname",
            "rtm_s_lastname",
            "rtm_s_email_address",
            "rtm_dt_birthdate",
            "rtm_i_age",
            "sal",
            "rtm_l_account",
            ""
        };
        public ColumnSet Columns => new ColumnSet(columns);

        private const string entityname = "contact";
        public string Entityname => entityname;

        public Entity Map(Contact contact)
        {
            var entity = new Entity(entityname);
            entity.Id = contact.ContactId.HasValue ? contact.ContactId.Value : Guid.Empty;
            entity["rtm_o_test"] = contact.Tests is null ? null : new OptionSetValueCollection(contact.Tests.Select(e => new OptionSetValue(e)).ToList());
            entity["rtm_o_test2"] = contact.Tests2 is null ? null : new OptionSetValueCollection(contact.Tests2.Select(e => new OptionSetValue(e)).ToList());
            entity["rtm_o_type"] = contact.ContactType.HasValue ? new OptionSetValue((int)contact.ContactType.Value) : null;
            entity["rtm_o_type2"] = contact.IntContantType.HasValue ? new OptionSetValue(contact.IntContantType.Value) : null;
            entity["rtm_s_firstname"] = contact.Firstname;
            entity["rtm_s_lastname"] = contact.Lastname;
            entity["rtm_s_email_address"] = contact.Email;
            entity["rtm_dt_birthdate"] = contact.Birthdate;
            entity["rtm_i_age"] = contact.Age;
            entity["sal"] = contact.Sallery.HasValue ? new Money(contact.Sallery.Value) : null;
            entity["rtm_l_account"] = contact.AccountId.HasValue ? new EntityReference("account", contact.AccountId.Value) : null;
            return entity;
        }

        public Contact Map(Entity entity)
        {
            if (entity?.LogicalName != entityname)
                throw new ArgumentException($"entity LogicalName expected to be {entityname} recived: {entity?.LogicalName}", "entity");
            var contact = new Contact();
            contact.ContactId = entity.Id;
            contact.Tests = entity.GetAttributeValue<OptionSetValueCollection>("rtm_o_test")?.Select(e => e.Value).ToArray();
            contact.Tests2 = entity.GetAttributeValue<OptionSetValueCollection>("rtm_o_test2")?.Select(e => e.Value).ToArray();
            contact.ContactType = (DebuggerClient.Enums.ContactType?)(entity.GetAttributeValue<OptionSetValue>("rtm_o_type")?.Value);
            contact.IntContantType = entity.GetAttributeValue<OptionSetValue>("rtm_o_type2")?.Value;
            contact.Firstname = entity.GetAttributeValue<string?>("rtm_s_firstname");
            contact.Lastname = entity.GetAttributeValue<string?>("rtm_s_lastname");
            contact.Email = entity.GetAttributeValue<string?>("rtm_s_email_address");
            contact.Birthdate = entity.GetAttributeValue<System.DateTime?>("rtm_dt_birthdate");
            contact.Age = entity.GetAttributeValue<int?>("rtm_i_age");
            contact.Sallery = entity.GetAttributeValue<Money>("sal")?.Value;
            contact.AccountId = entity.GetAttributeValue<EntityReference>("rtm_l_account")?.Id;
            var accountMapper = new AccountMapper();
            contact.Account = accountMapper.FromAliasedEntity(entity, "alias");
            return contact;
        }

        public Contact FromAliasedEntity(Entity entity, string alias)
        {
            var aliased = entity.GetAliasedEntity(alias);
            return Map(aliased);
        }
    }
}
