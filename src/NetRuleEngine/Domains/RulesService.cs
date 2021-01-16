using LazyCache;
using NetRuleEngine.Abstraction;
using System;
using System.Collections.Generic;


namespace NetRuleEngine.Domains
{
    public class RulesService<TObjectToMatch> : IRulesService<TObjectToMatch>
    {
        private readonly IRulesCompiler _compiler;
        private readonly IAppCache _cache;

        public RulesService(IRulesCompiler compiler, IAppCache cache)
        {
            _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }
        public static RulesService<TObjectToMatch> CreateDefault()
        {
            return new RulesService<TObjectToMatch>(new RulesCompiler(), new CachingService());
        }
        public BaseDataResponse<IEnumerable<Guid>> GetMatchingRules(TObjectToMatch objectToMatch, IEnumerable<RulesConfig> rulesConfig)
        {
            var matching = new HashSet<Guid>();
            foreach (var ruleConfig in rulesConfig)
            {
                try
                {
                    var compiledRule = _cache.GetOrAdd($"{GetType().Name}.{ruleConfig.Id}", () =>
                    {
                        return _compiler.CompileRule<TObjectToMatch>(ruleConfig);
                    });

                    if (compiledRule(objectToMatch))
                    {
                        matching.Add(ruleConfig.Id);
                    }
                }
                catch (Exception exc)
                {
                    return new BaseDataResponse<IEnumerable<Guid>> { Code = ErrorCode.TechnicalProblem, ErrorDescription = exc.Message, Data = matching };
                }
            }

            return new BaseDataResponse<IEnumerable<Guid>> { Data = matching };
        }
    }
}
