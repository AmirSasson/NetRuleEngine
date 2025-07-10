using System;

namespace NetRuleEngine.Abstraction
{
    public interface IRulesCompiler
    {
        public (Func<T, bool> CompliedRule, string RuleDescription) CompileRule<T>(RulesConfig rulesConfig);
    }
}