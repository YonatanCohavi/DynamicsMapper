using DynamicsMapper.Abstractions;

namespace XTests.Models
{
    public enum Color
    {
        Red,
        Blue,
        Green,
        BlueGreen,
        BlueYellow,
        GreenYellowGreen,
    }
    public enum ContactType
    {
        Partner = 1,
        Member = 2,
    }

    /// <summary>
    /// <see cref="ContactMapper"/>
    /// </summary>
    [CrmEntity("contact")]
    public class Contact
    {
        [CrmField("contactid", Mapping = MappingType.PrimaryId)]
        public Guid? Id { get; set; }
        [CrmField("rtm_mo_fav_colors", Mapping = MappingType.MultipleOptions)]
#if NETCOREAPP
        public Color[]? FavoriteColors { get; set; }
#elif NETFRAMEWORK
        public Color[] FavoriteColors { get; set; }
#endif
        [CrmField("rtm_mo_fav_colors_int", Mapping = MappingType.MultipleOptions)]
#if NETCOREAPP
        public int[]? FavoriteColorsInt { get; set; }
#elif NETFRAMEWORK
        public int[] FavoriteColorsInt { get; set; }
#endif
        [CrmField("rtm_o_type", Mapping = MappingType.Options)]
        public ContactType? ContactType { get; set; }
        [CrmField("rtm_o_type_int", Mapping = MappingType.Options)]
        public int? IntContantType { get; set; }
        [CrmField("rtm_s_firstname")]
#if NETCOREAPP
        public string? Firstname { get; set; }
#elif NETFRAMEWORK
        public string Firstname { get; set; }
#endif
        [CrmField("rtm_s_lastname")]
#if NETCOREAPP
        public string? Lastname { get; set; }
#elif NETFRAMEWORK
        public string Lastname { get; set; }
#endif
        [CrmField("rtm_s_email_address")]
#if NETCOREAPP
        public string? Email { get; set; }
#elif NETFRAMEWORK
        public string Email { get; set; }
#endif
        [CrmField("rtm_dt_birthdate")]
        public DateTime? Birthdate { get; set; }
        [CrmField("rtm_i_age")]
        public int? Age { get; set; }
        [CrmField("new_sallery", Mapping = MappingType.Money)]
        public decimal? Sallery { get; set; }
        [CrmField("rtm_l_account", Mapping = MappingType.Lookup, Target = "account")]
        public Guid? AccountId { get; set; }
        [CrmField("rtm_l_account", Mapping = MappingType.Formatted)]
#if NETCOREAPP
        public string? AccountName { get; set; }
#elif NETFRAMEWORK
        public string AccountName { get; set; }
#endif
        [CrmField("regardingobjectid", Mapping = MappingType.DynamicLookup)]
        public Guid? RegardingId { get; set; }
        [CrmField("regardingobjectid", Mapping = MappingType.DynamicLookupTarget)]
        public string RegardingIdTarget => "account";
        [CrmField("regardingobjectid", Mapping = MappingType.Formatted)]
        public string? RegardingName { get; set; }

        [CrmLink("break")]
        public string? Break { get; set; }
    }

    public class Fake
    {

    }
}
