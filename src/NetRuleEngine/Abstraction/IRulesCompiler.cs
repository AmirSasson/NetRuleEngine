using System;

namespace NetRuleEngine.Abstraction
{
    public interface IRulesCompiler
    {
        public Func<T, bool> CompileRule<T>(RulesConfig rulesConfig);
    }
}
