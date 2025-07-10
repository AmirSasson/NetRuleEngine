using LazyCache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetRuleEngine.Abstraction;
using System;
using System.Collections.Generic;


namespace NetRuleEngine.Domains
{
    public class RulesService<TObjectToMatch> : IRulesService<TObjectToMatch>
    {
        private readonly IRulesCompiler _compiler;
        private readonly IAppCache _cache;
        private readonly ILogger _logger;

        public RulesService(IRulesCompiler compiler, IAppCache cache, ILogger logger)
        {
            _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger;
        }
        public static RulesService<TObjectToMatch> CreateDefault()
        {
            return new RulesService<TObjectToMatch>(new RulesCompiler(), new CachingService(), NullLogger.Instance);
        }
        public BaseDataResponse<IEnumerable<Guid>> GetMatchingRules(TObjectToMatch objectToMatch, IEnumerable<RulesConfig> rulesConfig)
        {
            var matching = new HashSet<Guid>();
            foreach (var ruleConfig in rulesConfig)
            {
                try
                {
                    var compiledRule = _cache.GetOrAdd($"{GetType().Name}.{ruleConfig.CacheKey}", () =>
                    {
                        var (CompliedRule, RuleDescription) = _compiler.CompileRule<TObjectToMatch>(ruleConfig);
                        _logger.LogInformation("Compiled rule {RuleID}: {RuleDescription}", ruleConfig.Id, RuleDescription);
                        return CompliedRule;
                    });

                    if (compiledRule(objectToMatch))
                    {
                        matching.Add(ruleConfig.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Error when running {RuleID}: {RuleDescription}", ruleConfig.Id);
                    return new BaseDataResponse<IEnumerable<Guid>> { Code = ErrorCode.TechnicalProblem, ErrorDescription = ex.Message, Data = matching };
                }
            }

            return new BaseDataResponse<IEnumerable<Guid>> { Data = matching };
        }
    }
}
