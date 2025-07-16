using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NetRuleEngine.Abstraction
{
    /// <summary>
    /// Rule Root
    /// </summary>
    public class RulesConfig
    {
        public Guid Id { get; set; }

        [JsonIgnore]
        public string CacheKey => ToJson().GetHashCode().ToString();

        [JsonConverter(typeof(StringEnumConverter))]
        public InternalRuleOperatorType RulesOperator { get; set; }

        public IEnumerable<RuleNode> RulesGroups { get; set; }

        public static RulesConfig FromJson(string json)
        {
            var settings = new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter() }
            };
            return JsonConvert.DeserializeObject<RulesConfig>(json, settings);
        }

        public string ToJson()
        {
            var settings = new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(this, settings);
        }
    }
}
