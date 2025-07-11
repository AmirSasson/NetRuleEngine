using System;

namespace NetRuleEngine.Domains
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RulePredicatePropertyAttribute : Attribute
    {
        public string Name { get; }
        public RulePredicatePropertyAttribute(string name)
        {
            Name = name;
        }
    }
}