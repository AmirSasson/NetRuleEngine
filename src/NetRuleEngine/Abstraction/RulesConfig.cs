using System;
using System.Collections.Generic;
using System.Text.Json;
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
        public Guid Id { get; set; }

        [JsonIgnore]
        public string CacheKey => ToJson().GetHashCode().ToString();

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Rule.InterRuleOperatorType RulesOperator { get; set; }

        public IEnumerable<RulesGroup> RulesGroups { get; set; }

        public static RulesConfig FromJson(string json)
        {
            return JsonSerializer.Deserialize<RulesConfig>(json);
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
