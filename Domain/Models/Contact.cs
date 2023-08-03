using DynamicsMapper.Abstractions;

namespace Domain.Models
{
    [CrmEntity("contact")]
    public partial class Contact
    {
        [CrmField("activityid", Mapping = MappingType.PrimaryId)]
        public Guid? ContactId { get; set; }
        [CrmField("rtm_o_test", Mapping = MappingType.MultipleOptions)]
        public int[]? Tests { get; set; }
        [CrmField("firstname")]
        public string? Firstname { get; set; }
        [CrmField("rtm_s_lastname")]
        public string? Lastname { get; set; }
        [CrmField("rtm_s_email_address")]
        public string? Email { get; set; }
        [CrmField("rtm_dt_birthdate")]
        public DateTime? Birthdate { get; set; }
        [CrmField("rtm_i_age")]
        public int? Age { get; set; }
        [CrmField("rtm_l_account", Mapping = MappingType.Money)]
        public decimal? Sallery { get; set; }
        [CrmField("rtm_l_account", Target = "account")]
        public Guid? AccountId { get; set; }
        [CrmField("rtm_l_account", Mapping = MappingType.Formatted)]
        public string? AccountName { get; }
    }
}
