using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace NetRuleEngine.Abstraction
{
    public class Rule : RuleNode
    {
        ///
        /// Denotes the rules predicate (e.g. Name); comparison operator(e.g. ExpressionType.GreaterThan); value (e.g. "Cole")
        /// 
        public string ComparisonPredicate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ComparisonOperatorType ComparisonOperator { get; set; }
        public string ComparisonValue { get; set; }
        public TypeCode? PredicateType { get; set; }

        /// 
        /// The rule method that 
        /// 
        public Rule(string comparisonPredicate, ComparisonOperatorType comparisonOperator, string comparisonValue, TypeCode? convertToType = null)
        {
            ComparisonPredicate = comparisonPredicate;
            ComparisonOperator = comparisonOperator;
            ComparisonValue = comparisonValue;
            PredicateType = convertToType;
        }

        public Rule() { }
    }
}
