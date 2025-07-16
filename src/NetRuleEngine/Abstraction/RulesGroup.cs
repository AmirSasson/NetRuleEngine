using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace NetRuleEngine.Abstraction
{
    public class RulesGroup : RuleNode
    {
        public override string Type => "RulesGroup";

        [JsonProperty("Operator", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Include, Order = int.MinValue)]
        [JsonConverter(typeof(StringEnumConverter))]
        public InternalRuleOperatorType Operator { get; set; }

        public IEnumerable<RuleNode> Rules { get; set; }
    }
}
