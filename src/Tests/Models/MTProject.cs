using System;

namespace Tests.Models
{
    public class MTProject
    {
        public Guid Id { get; set; }
        public DateTime Createdon { get; set; }
        public DateTime Modifiedon { get; set; }
        public DateTime? OpenProject { get; set; }
        public Guid? Secretary { get; set; }
        public string Name { get; set; }
        public string ProjectNumber { get; set; }
        public int? Statuscode { get; set; }
    }
}
