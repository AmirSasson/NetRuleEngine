 NetRuleEngine
 ==============
C# simple Rule Engine. High performance object rule matching. Support various complex grouped predeicates.
available on [nuget](https://www.nuget.org/packages/NetRuleEngine/)

#### Simple usage:

```
    var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());
            
    var matching = engine.GetMatchingRules(
        new TestModel { NumericField = 5 },
        new[] {
            new RulesConfig {
                Id = 1,
                RulesOperator = Rule.InterRuleOperatorType.And,
                RulesGroups = new RulesGroup[] {
                    new RulesGroup {
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        Rules = new[] {
                            new Rule { ComparisonOperator = Rule.ComparisonOperatorType.Equal, ComparisonValue = 5.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                        }
                    }
                }
            }
        });
```

- depenent on LazyCache to store compiles rules.
- compiles Expression Trees into dynamic cached code to support high performance usage.

Features:
- composite objects
- enums
- string
- numbers
- datetime
- Dictionaries
- collections

and many more.
S
ee units test for full usage scenarios.