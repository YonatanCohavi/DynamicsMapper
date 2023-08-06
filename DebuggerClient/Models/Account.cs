using DynamicsMapper.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerClient.Models
{
    [CrmEntity("account")]
    public class Account
    {
        [CrmField("accountid",Mapping = MappingType.PrimaryId)]
        public Guid AccountId { get; set; }
    }
}
