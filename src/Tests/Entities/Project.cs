using DynamicsMapper.Abstractions;

namespace Tests.Entities
{
    /// <summary> פרויקטים <para>rtm_projects</para> </summary>
    /// <see cref="ProjectMapper"/>
    [CrmEntity("rtm_projects")]
    public class Project
    {
        [CrmField("rtm_projectsid", Mapping = MappingType.PrimaryId)]
        public Guid? Id { get; set; }
        /// <summary> Created On <para>createdon</para> </summary>
        [CrmField("createdon")]
        public DateTime Createdon { get; set; }
        /// <summary> Modified On <para>modifiedon</para> </summary>
        [CrmField("modifiedon")]
        public DateTime Modifiedon { get; set; }
        /// <summary> תאריך פתיחת פרויקט במערכות WXG <para>rtm_d_open_project</para> </summary>
        [CrmField("rtm_d_open_project")]
        public DateTime? OpenProject { get; set; }
        /// <summary> מזכירה <para>rtm_id_secretary</para> </summary>
        [CrmField("rtm_id_secretary", Mapping = MappingType.Lookup, Target = "rtm_wxg_empl")]
        public Guid? SecretaryId { get; set; }
        /// <summary> מנהל חטיבה <para>rtm_id_secretary</para> </summary>
        [CrmField("rtm_id_division_manager", Mapping = MappingType.Lookup, Target = "rtm_wxg_empl")]
        public Guid? DivitionManagerId { get; set; }
        /// <summary> שם ומספר פרויקט <para>rtm_name</para> </summary>
        [CrmField("rtm_name")]
        public string? FullName { get; set; }
        /// <summary> מספר פרויקט <para>rtm_s_project</para> </summary>
        [CrmField("rtm_s_project")]
        public string? ProjectNumber { get; set; }
        /// <summary> סטטוס <para>statuscode</para> </summary>
        [CrmField("statuscode", Mapping = MappingType.Options)]
        public int Statuscode { get; set; }
        [CrmField("statuscode", Mapping = MappingType.Formatted)]
        public string? StatuscodeName { get; set; }
        /// <summary> (לא בשימוש)שלב הפרויקט <para>rtm_o_project_level</para> </summary>
        [CrmField("rtm_o_project_level", Mapping = MappingType.Formatted)]
        public string? ProjectLevelName { get; set; }
        /// <summary> (לא בשימוש(חטיבה <para>rtm_o_unit</para> </summary>
        [CrmField("rtm_o_unit", Mapping = MappingType.Formatted)]
        public string? UnitName { get; set; }
        /// <summary> (לא בשימוש)תחום ראשי <para>rtm_o_main_confines</para> </summary>
        [CrmField("rtm_o_main_confines", Mapping = MappingType.Formatted)]
        public string? MainConfinesName { get; set; }
        [CrmField("rtm_s_marketing_project")]
        public string? MarketingProject { get; set; }
        [CrmField("rtm_s_name")]
        public string? Name { get; set; }

        /// <summary> לקוח 1 <para>rtm_id_account1</para> </summary>
        [CrmField("rtm_id_account1", Mapping = MappingType.Lookup, Target = "account")]
        public Guid? Account1Id { get; set; }
        [CrmField("rtm_id_account1", Mapping = MappingType.Formatted)]
        public string? Account1Name { get; set; }
        /// <summary> לקוח 2 <para>rtm_id_account2</para> </summary>
        [CrmField("rtm_id_account2", Mapping = MappingType.Lookup, Target = "account")]
        public Guid? Account2Id { get; set; }
        [CrmField("rtm_id_account2", Mapping = MappingType.Formatted)]
        public string? Account2Name { get; set; }
        /// <summary> עיר <para>rtm_id_city</para> </summary>
        [CrmField("rtm_id_city", Mapping = MappingType.Lookup, Target = "rtm__id_cities")]
        public Guid? CityId { get; set; }
        [CrmField("rtm_id_city", Mapping = MappingType.Formatted)]
        public string? CityName { get; set; }
        /// <summary> רחוב <para>rtm_s_address</para> </summary>
        [CrmField("rtm_s_address")]
        public string? Street { get; set; }
        /// <summary> תיאור פרויקט <para>rtm_s_description</para> </summary>
        [CrmField("rtm_s_description")]
        public string? Description { get; set; }
        /// <summary> מספר בית <para>rtm_s_house_number</para> </summary>
        [CrmField("rtm_s_house_number")]
        public string? HouseNumber { get; set; }
    }
}
