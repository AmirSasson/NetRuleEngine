using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetRuleEngine.Abstraction
{
    public class RulesConfig
    {
        public class RulesGroup
        {
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public Rule.InterRuleOperatorType RulesOperator { get; set; }
            public IEnumerable<Rule> Rules { get; set; }
        }
        public int Id { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Rule.InterRuleOperatorType RulesOperator { get; set; }

        public IEnumerable<RulesGroup> RulesGroups { get; set; }
    }
}
