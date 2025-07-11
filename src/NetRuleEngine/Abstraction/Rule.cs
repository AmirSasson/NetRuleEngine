using System;
using System.Text.Json.Serialization;

namespace NetRuleEngine.Abstraction
{
    ///
    /// The Rule type
    /// 
    public class Rule

    {
        public enum ComparisonOperatorType
        {
            //
            // Summary:
            //     A node that represents an equality comparison, such as (a == b) in C# or (a =
            //     b) in Visual Basic.
            Equal = 13,
            //     A "greater than" comparison, such as (a > b).
            GreaterThan = 15,
            //
            // Summary:
            //     A "greater than or equal to" comparison, such as (a >= b).
            GreaterThanOrEqual = 16,
            //
            // Summary:
            //     A "less than" comparison, such as (a < b).
            LessThan = 20,
            //
            // Summary:
            //     A "less than or equal to" comparison, such as (a <= b).
            LessThanOrEqual = 21,
            //
            // Summary:
            //     An inequality comparison, such as (a != b) in C# or (a <> b) in Visual Basic.
            NotEqual = 35,

            //
            // Summary:
            //     A true condition value.
            IsTrue = 83,
            //
            // Summary:
            //     A false condition value.
            IsFalse = 84,


            CollectionContainsAnyOf = 900,
            CollectionNotContainsAnyOf = 901,
            CollectionContainsAll = 902,

            In = 1000,
            NotIn = 1001,

            // ignore case
            StringStartsWith = 1002,

            // ignore case
            StringEndsWith = 1003,

            // ignore case
            StringContains = 1004,

            // ignore case
            StringNotContains = 1005,
            StringMatchesRegex = 1006,
            StringEqualsCaseInsensitive = 1007,
            StringNotEqualsCaseInsensitive = 1008,
            StringNullOrEmpty = 1009,
            StringNotNullOrEmpty = 1010
        }

        public enum InterRuleOperatorType
        {
            And,
            Or
        }
        ///
        /// Denotes the rules predictate (e.g. Name); comparison operator(e.g. ExpressionType.GreaterThan); value (e.g. "Cole")
        /// 
        public string ComparisonPredicate { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
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
