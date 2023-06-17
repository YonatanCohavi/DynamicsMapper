using Generator.Attributes;
using Microsoft.Xrm.Sdk.Query;

namespace ConsoleClient.Models
{
    public enum ContactType
    {
        Partner = 1,
        Member = 2,
    }
    [CrmEntity("contact")]
    public partial class Contact
    {
        [CrmOptions("rtm_o_type")]
        public ContactType? ContantType { get; set; }
        [CrmOptions("rtm_o_type")]
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
        [CrmMoney("rtm_m_sallery")]
        public decimal? Sallery { get; set; }
        [CrmReference("rtm_l_account", "account")]
        public Guid AccountId { get; set; }
        [CrmFormatted("rtm_l_account")]
        public string? AccountName { get; set; }
    }
}
