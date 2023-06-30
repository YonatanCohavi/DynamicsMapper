using DynamicsMapper.Abstractions;

namespace ConsoleClient
{
    [CrmEntity("account")]
    public partial class Account
    {
        [CrmField("rtm_s_firstname")]
        public string? Firstname { get; set; }
        [CrmField("rtm_b_isvalid")]
        public bool? IsValid { get; set; }

    }
}
