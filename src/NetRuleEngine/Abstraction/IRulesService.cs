using System;
using System.Collections.Generic;

namespace NetRuleEngine.Abstraction
{
    public interface IRulesService<TObjectToMatch>
    {
        /// <summary>
        /// Returns a collection of rule IDs that match the given object based on the provided rules configuration.
        /// If an error occurs during rule evaluation, it returns a response with an error code and description.      
        /// The method can be extended to support different matching modes, such as returning all matching rules or just the first match.
        /// </summary>
        /// <param name="objectToMatch">object to match rules </param>
        /// <param name="rulesConfig">rules list by priority (the first is the highest priority)</param>
        /// <param name="matchingMode">default to ReturnFirstMatchingRule</param>        
        /// <returns>All Matching rules for objectToMatch in case of Mode.ReturnAllMatchingRules, or the first matching rule in case of Mode.ReturnFirstMatchingRule.</returns>        
        BaseDataResponse<IEnumerable<Guid>> GetMatchingRules(TObjectToMatch objectToMatch, IEnumerable<RulesConfig> rulesConfig, Mode matchingMode = Mode.ReturnAllMatchingRules);
    }

    public enum Mode
    {
        ReturnAllMatchingRules,
        ReturnFirstMatchingRule
    }
}