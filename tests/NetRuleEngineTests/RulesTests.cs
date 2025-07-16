using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetRuleEngine.Abstraction;
using NetRuleEngine.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NetRuleEngineTests
{
    public class RulesTests
    {
        private readonly ILogger _logger;
        public RulesTests()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            _logger = loggerFactory.CreateLogger<RulesTests>();
        }

        // First add the new nested rules tests
        [Fact]
        public void GetMatchingRules_NestedGroups_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "example", NumericField = 15 },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.Or,
                                Rules = [
                                    new Rule {
                                        ComparisonOperator = ComparisonOperatorType.Equal,
                                        ComparisonValue = "not matching",
                                        ComparisonPredicate = nameof(TestModel.TextField)
                                    },
                                    new RulesGroup {
                                        Operator = InternalRuleOperatorType.And,
                                        Rules = [
                                            new Rule {
                                                ComparisonOperator = ComparisonOperatorType.Equal,
                                                ComparisonValue = "example",
                                                ComparisonPredicate = nameof(TestModel.TextField)
                                            },
                                            new Rule {
                                                ComparisonOperator = ComparisonOperatorType.GreaterThan,
                                                ComparisonValue = "10",
                                                ComparisonPredicate = nameof(TestModel.NumericField)
                                            }
                                        ]
                                    }
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }


        //{"Id":"1b82df80-d215-4fc1-bd49-7329e7f584e1","RulesOperator":"And","RulesGroups":[{"$type":"RulesGroup","Rules":[{"$type":"Rule","Operator":"And","ComparisonPredicate":"TextField","ComparisonOperator":"Equal","ComparisonValue":"not matching","PredicateType":null},{"$type":"RulesGroup","Rules":[{"$type":"RulesGroup","Rules":[{"$type":"Rule","Operator":"And","ComparisonPredicate":"TextField","ComparisonOperator":"Equal","ComparisonValue":"example","PredicateType":null}],"Operator":"Or","ComparisonPredicate":null,"ComparisonOperator":0,"ComparisonValue":null,"PredicateType":null},{"$type":"RulesGroup","Rules":[{"$type":"Rule","Operator":"And","ComparisonPredicate":"NumericField","ComparisonOperator":"GreaterThan","ComparisonValue":"10","PredicateType":null}],"Operator":"Or","ComparisonPredicate":null,"ComparisonOperator":0,"ComparisonValue":null,"PredicateType":null}],"Operator":"And","ComparisonPredicate":null,"ComparisonOperator":0,"ComparisonValue":null,"PredicateType":null}],"Operator":"Or","ComparisonPredicate":null,"ComparisonOperator":0,"ComparisonValue":null,"PredicateType":null}]}


        [Fact]
        public void GetMatchingRules_NestedGroups_RuleNotReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "example", NumericField = 5 },  // NumericField < 10 should cause the nested group to not match
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.Or,
                                Rules = [
                                    new Rule {
                                        ComparisonOperator = ComparisonOperatorType.Equal,
                                        ComparisonValue = "not matching",
                                        ComparisonPredicate = nameof(TestModel.TextField)
                                    },
                                    new RulesGroup {
                                        Operator = InternalRuleOperatorType.And,
                                        Rules = [
                                            new Rule {
                                                ComparisonOperator = ComparisonOperatorType.Equal,
                                                ComparisonValue = "example",
                                                ComparisonPredicate = nameof(TestModel.TextField)
                                            },
                                            new Rule {
                                                ComparisonOperator = ComparisonOperatorType.GreaterThan,
                                                ComparisonValue = "10",
                                                ComparisonPredicate = nameof(TestModel.NumericField)
                                            }
                                        ]
                                    }
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Empty(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_DeepNestedGroups_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "example", NumericField = 15 },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.Or,
                                Rules = [
                                    new Rule {
                                        ComparisonOperator = ComparisonOperatorType.Equal,
                                        ComparisonValue = "not matching",
                                        ComparisonPredicate = nameof(TestModel.TextField)
                                    },
                                    new RulesGroup {
                                        Operator = InternalRuleOperatorType.And,
                                        Rules = [
                                            new RulesGroup {
                                                Operator = InternalRuleOperatorType.Or,
                                                Rules = [
                                                    new Rule {
                                                        ComparisonOperator = ComparisonOperatorType.Equal,
                                                        ComparisonValue = "example",
                                                        ComparisonPredicate = nameof(TestModel.TextField)
                                                    }
                                                ]
                                            },
                                            new RulesGroup {
                                                Operator = InternalRuleOperatorType.Or,
                                                Rules = [
                                                    new Rule {
                                                        ComparisonOperator = ComparisonOperatorType.GreaterThan,
                                                        ComparisonValue = "10",
                                                        ComparisonPredicate = nameof(TestModel.NumericField)
                                                    }
                                                ]
                                            }
                                        ]
                                    }
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_DeserializedRule_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            var rule = new RulesConfig
            {
                Id = Guid.NewGuid(),
                RulesOperator = InternalRuleOperatorType.And,
                RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.Or,
                                Rules = [
                                    new Rule {
                                        ComparisonOperator = ComparisonOperatorType.Equal,
                                        ComparisonValue = "not matching",
                                        ComparisonPredicate = nameof(TestModel.TextField)
                                    },
                                    new RulesGroup {
                                        Operator = InternalRuleOperatorType.And,
                                        Rules = [
                                            new RulesGroup {
                                                Operator = InternalRuleOperatorType.Or,
                                                Rules = [
                                                    new Rule {
                                                        ComparisonOperator = ComparisonOperatorType.Equal,
                                                        ComparisonValue = "example",
                                                        ComparisonPredicate = nameof(TestModel.TextField)
                                                    }
                                                ]
                                            },
                                            new RulesGroup {
                                                Operator = InternalRuleOperatorType.Or,
                                                Rules = [
                                                    new Rule {
                                                        ComparisonOperator = ComparisonOperatorType.GreaterThan,
                                                        ComparisonValue = "10",
                                                        ComparisonPredicate = nameof(TestModel.NumericField)
                                                    }
                                                ]
                                            }
                                        ]
                                    }
                                ]
                            }
                        ]
            };

            var stringifiedRule = rule.ToJson(); // Serialize to JSON to simulate deserialization
            var deserializedRule = RulesConfig.FromJson(stringifiedRule);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "example", NumericField = 15 },
                [
                    deserializedRule
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_DeserializedRule_RuleMatch()
        {
            var serializedRule = /*lang=json,strict*/ """
{
  "Id": "d952df97-7d54-45db-acf4-90f723e7bdf0",
  "RulesOperator": "And",
  "RulesGroups": [
    {
        "$type": "RulesGroup",
        "Rules": [
            {
                "ComparisonPredicate": "TextField",
                "ComparisonOperator": "Equal",
                "ComparisonValue": "not matching"
            },
            {
                "$type": "RulesGroup",
                "Rules": [
                    {
                        "$type": "RulesGroup",
                        "Rules": [
                            {                                
                                "ComparisonPredicate": "TextField",
                                "ComparisonOperator": "Equal",
                                "ComparisonValue": "example"
                            },
                            {                                
                                "ComparisonPredicate": "TextField",
                                "ComparisonOperator": "StringStartsWith",
                                "ComparisonValue": "ex"
                            }
                        ],
                        "Operator": "And"
                    },
                    {
                        "$type": "RulesGroup",
                        "Rules": [
                            {                            
                                "ComparisonPredicate": "NumericField",
                                "ComparisonOperator": "GreaterThan",
                                "ComparisonValue": "10"
                            },
                            {                            
                                "ComparisonPredicate": "NumericField",
                                "ComparisonOperator": "GreaterThanOrEqual",
                                "ComparisonValue": "15"
                            }
                        ],
                        "Operator": "And"
                    }
                ],
                "Operator": "And"
            }
      ],
      "Operator": "Or"
    }
  ]
}
""";

            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);
            var deserializedRule = RulesConfig.FromJson(serializedRule);


            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "example", NumericField = 15 },
                [
                    deserializedRule
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }




        // Then include all the original tests, updated to use RulesGroup.Operator instead of RulesOperator
        [Theory]
        [InlineData(5, ComparisonOperatorType.Equal, 5, true)]
        [InlineData(5, ComparisonOperatorType.LessThan, 4, true)]
        [InlineData(5, ComparisonOperatorType.LessThan, 6, false)]
        [InlineData(5, ComparisonOperatorType.GreaterThan, 6, true)]
        public void GetMatchingRules_NumericValueMatch_ShouldMatchByOperatorAndValue(int ruleVal, ComparisonOperatorType op, int objectVal, bool shouldMatch)
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), _logger);

            // Act
            var numericValueTest = objectVal;
            var matching = engine.GetMatchingRules(
                new TestModel { NumericField = numericValueTest },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = op, ComparisonValue = ruleVal.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                ]
                            }
                        ]
                    }
                ]);

            // Assert  
            if (shouldMatch)
            {
                Assert.Single(matching.Data);
            }
            else
            {
                Assert.Empty(matching.Data);
            }
        }

        [Fact]
        public void GetMatchingRules_NumericValueNotMatch_RuleNotReturned()
        {
            // Arrange
            IRulesService<TestModel> engine = RulesService<TestModel>.CreateDefault();

            // Act
            var numericValueTest = 5;
            var numericValueOtherValue = 6;
            var matching = engine.GetMatchingRules(
                new TestModel { NumericField = numericValueTest },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = ComparisonOperatorType.Equal, ComparisonValue = numericValueOtherValue.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Empty(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_StringStartsWithMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "SomePrefixBlahBlah" },
                [
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = ComparisonOperatorType.StringStartsWith, ComparisonValue = "someprefix",  ComparisonPredicate = nameof(TestModel.TextField) }
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_MultiRuleAndMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "SomePrefixBlahBlah", NumericField = 10 },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = ComparisonOperatorType.StringStartsWith, ComparisonValue = "someprefix",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = ComparisonOperatorType.GreaterThan, ComparisonValue = 4.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_CaluculatedCollectionContainsAnyOfMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { CompositeCollection = [new TestModel.CompositeInnerClass { NumericField = 10 }] },
                [
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = ComparisonOperatorType.CollectionContainsAnyOf, ComparisonValue = "10|11|12",  ComparisonPredicate = $"{nameof(TestModel.CaluculatedCollection)}"}
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_CaluculatedCollectionNotContainsAnyOfNotMatch_RuleNotReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { CompositeCollection = [new TestModel.CompositeInnerClass { NumericField = 10 }] },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = ComparisonOperatorType.CollectionNotContainsAnyOf, ComparisonValue = "10|11|12",  ComparisonPredicate = $"{nameof(TestModel.CaluculatedCollection)}"}
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Empty(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_PrimitiveCollectionMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { PrimitivesCollection = [1, 2, 4, 5] },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = ComparisonOperatorType.CollectionContainsAll, ComparisonValue = "1|2", ComparisonPredicate = $"{nameof(TestModel.PrimitivesCollection)}"}
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_PrimitiveCollectionNoMatch_RuleNotReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { PrimitivesCollection = [1, 2, 4, 5] },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = ComparisonOperatorType.CollectionContainsAll, ComparisonValue = "1|2|10", ComparisonPredicate = $"{nameof(TestModel.PrimitivesCollection)}"}
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Empty(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_ComparisonPredicateNameAttribute_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModelWithRulePredicatePropertyAttribute>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModelWithRulePredicatePropertyAttribute
                {
                    FirstName = "John",
                    PersonDetails = new Dictionary<string, string> { { "SSN", "123456789" } },
                    PersonAddress = new AddressModel { HomeAddress = "over the rainbow", StreetAddress = "1st" }
                },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = ComparisonOperatorType.StringEqualsCaseInsensitive, ComparisonValue = "john", ComparisonPredicate = "first_name"},
                                    new Rule { ComparisonOperator = ComparisonOperatorType.Equal, ComparisonValue = "123456789", ComparisonPredicate = "userDetails[SSN]"},
                                    new Rule { ComparisonOperator = ComparisonOperatorType.Equal, ComparisonValue = "over the rainbow", ComparisonPredicate = "userAddress.home_address"},
                                    new Rule { ComparisonOperator = ComparisonOperatorType.Equal, ComparisonValue = "1st", ComparisonPredicate = "userAddress.StreetAddress"}
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_FirstMatchingRule_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModelWithRulePredicatePropertyAttribute>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModelWithRulePredicatePropertyAttribute
                {
                    FirstName = "John",
                },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = ComparisonOperatorType.StringEqualsCaseInsensitive, ComparisonValue = "john", ComparisonPredicate = "first_name"}
                                ]
                            }
                        ]
                    },
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = InternalRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                Operator = InternalRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = ComparisonOperatorType.StringContains, ComparisonValue = "jo", ComparisonPredicate = "first_name"}
                                ]
                            }
                        ]
                    }
                ], Mode.ReturnFirstMatchingRule);

            // Assert            
            Assert.Single(matching.Data);
        }
    }

    public class TestModelWithRulePredicatePropertyAttribute : TestModel
    {
        [RulePredicateProperty("first_name")]
        public string FirstName { get; set; }

        [RulePredicateProperty("userDetails")]
        public Dictionary<string, string> PersonDetails { get; set; }

        [RulePredicateProperty("userAddress")]
        public AddressModel PersonAddress { get; set; }
    }

    public class AddressModel
    {
        [RulePredicateProperty("home_address")]
        public string HomeAddress { get; set; }

        // no Aliasing example
        public string StreetAddress { get; set; }
    }
}
