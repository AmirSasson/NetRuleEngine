using JsonSubTypes;
using Newtonsoft.Json;

namespace NetRuleEngine.Abstraction
{
    [JsonConverter(typeof(JsonSubtypes), "$type")]
    [JsonSubtypes.KnownSubType(typeof(RulesGroup), "RulesGroup")]
    [JsonSubtypes.KnownSubType(typeof(Rule), "Rule")]
    [JsonSubtypes.FallBackSubType(typeof(Rule))]
    public abstract class RuleNode
    {
        [JsonProperty("$type")]
        public virtual string Type => "Rule";

        // only serialize Type if it is not the default "Rule"        
        public bool ShouldSerializeType()
        {
            return Type != "Rule";
        }

    }
}
