﻿using DebuggerClient.Enums;
using DynamicsMapper.Abstractions;

namespace DebuggerClient.Models
{
    [CrmEntity("contact")]
    public class Contact
    {
        const string AccountAlias = "account";
        [CrmField("contactid", Mapping = MappingType.PrimaryId)]
        public Guid? ContactId { get; set; }
        [CrmField("rtm_o_test", Mapping = MappingType.MultipleOptions)]
        public int[]? Tests { get; set; }
        [CrmField("rtm_o_test2", Mapping = MappingType.MultipleOptions)]
        public ContactType[]? Tests2 { get; set; }
        [CrmField("rtm_o_type", Mapping = MappingType.Options)]
        public ContactType? ContactType { get; set; }
        [CrmField("rtm_o_type2", Mapping = MappingType.Options)]
        public int? IntContantType { get; set; }
        [CrmField("rtm_s_firstname")]
        public string? Firstname { get; set; }
        [CrmField("rtm_s_lastname")]
        public string? Lastname { get; set; }
        [CrmField("rtm_s_email_address")]
        public string? Email { get; set; }
        [CrmField("rtm_dt_birthdate")]
        public DateTime? Birthdate { get; set; }
        [CrmField("rtm_i_age")]
        public int? Age { get; set; }
        [CrmField("sal", Mapping = MappingType.Money)]
        public decimal? Sallery { get; set; }
        [CrmField("rtm_l_account", Target = "account")]
        public Guid? AccountId { get; set; }
        [CrmField("rtm_l_account", Mapping = MappingType.Formatted)]
        public string? AccountName { get; set; }
        //[CrmLink(AccountAlias)]
        //public Account? Account { get; set; }

    }
}
