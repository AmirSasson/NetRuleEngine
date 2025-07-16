namespace NetRuleEngine.Abstraction
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

        /// <summary>
        /// the ComparisonValue value should be a string with pipe (|) separated values like : 1|2|3
        /// </summary>
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
}
