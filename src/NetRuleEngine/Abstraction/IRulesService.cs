using System;
using System.Collections.Generic;

namespace NetRuleEngine.Abstraction
{
    public interface IRulesService<TObjectToMatch>
    {
        BaseDataResponse<IEnumerable<Guid>> GetMatchingRules(TObjectToMatch objectToMatch, IEnumerable<RulesConfig> rulesConfig);
    }
}