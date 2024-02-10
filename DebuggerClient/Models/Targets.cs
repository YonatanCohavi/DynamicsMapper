using DynamicsMapper.Abstractions;

namespace DebuggerClient.Models
{
    public struct RegardingTarget
    {
        public RegardingTarget(string value) { Value = value; }
        public string Value { get; set; }
    }
    public class Targets
    {

        public Targets(RegardingTarget regarding, string ownerId)
        {
            Regarding = regarding;
            OwnerId = ownerId;
        }

        public RegardingTarget Regarding { get; set; }
        public string OwnerId { get; set; } = string.Empty;

        public static implicit operator DynamicsMappingsTargets(Targets targets)
        {
            return new DynamicsMappingsTargets
            {
                {
                    "regardingobjectid",
                    targets.Regarding.Value
                },
                {
                    "ownerid",
                    targets.OwnerId
                },
            };
        }
    }
}
