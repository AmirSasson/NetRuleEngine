﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NetRuleEngine.Abstraction;
using NetRuleEngine.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static NetRuleEngine.Abstraction.RulesConfig;

namespace NetRuleEngineTests
{
    public class RulesTests
    {
        private readonly ILogger _logger;
        public RulesTests()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole(); // This line now works with the added namespace
            });

            _logger = loggerFactory.CreateLogger<RulesTests>();
        }

        [Theory]
        [InlineData(5, Rule.ComparisonOperatorType.Equal, 5, true)]
        [InlineData(5, Rule.ComparisonOperatorType.LessThan, 4, true)]
        [InlineData(5, Rule.ComparisonOperatorType.LessThan, 6, false)]
        [InlineData(5, Rule.ComparisonOperatorType.GreaterThan, 6, true)]
        public void GetMatchingRules_NumericValueMatch_ShouldMatchByOperatorAndValue(int ruleVal, Rule.ComparisonOperatorType op, int objectVal, bool shouldMatch)
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
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.And,
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
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.Equal, ComparisonValue = numericValueOtherValue.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
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
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "someprefix",  ComparisonPredicate = nameof(TestModel.TextField) }
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
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "someprefix",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThan, ComparisonValue = 4.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_MultiRuleOrMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "SomePrefixBlahBlah", NumericField = 10 },
                [
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "NOT MATCHING PREFIX",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThan, ComparisonValue = 4.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_MultiGroupAnMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var ruleConfig =
                    new RulesConfig
                    {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "NOT MATCHING PREFIX",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThan, ComparisonValue = 4.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                ]
                            },
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "SomePrefix",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThan, ComparisonValue = 55.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                ]
                            }
                        ]
                    };
            var text = ruleConfig.ToJson();
            var deserializedRules = FromJson(text);

            var matching = engine.GetMatchingRules(
                    new TestModel { TextField = "SomePrefixBlahBlah", NumericField = 10 },
                    [deserializedRules]);
            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_MultiGroupFirstGroupNotMatch_RuleNotReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "SomePrefixBlahBlah", NumericField = 10 },
                [
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {                // this group does not match!
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "NOT MATCHING PREFIX",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.LessThan, ComparisonValue = 4.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                ]
                            },
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "SomePrefix",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThan, ComparisonValue = 55.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Empty(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_CompositePropertyMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { Composit = new TestModel.CompositeInnerClass { NumericField = 10 } },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThanOrEqual, ComparisonValue = 4.ToString(),  ComparisonPredicate = $"{nameof(TestModel.Composit)}.{nameof(TestModel.Composit.NumericField)}"}
                            ]
                        }
                    ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_CaluculatedCOllectionCollectionContainsAnyOfMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { CompositeCollection = [new TestModel.CompositeInnerClass { NumericField = 10 }] },
                [
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.CollectionContainsAnyOf, ComparisonValue = "10|11|12",  ComparisonPredicate = $"{nameof(TestModel.CaluculatedCollection)}"}
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
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.CollectionNotContainsAnyOf, ComparisonValue = "10|11|12",  ComparisonPredicate = $"{nameof(TestModel.CaluculatedCollection)}"}
                            ]
                        }
                    ]
                    }
                ]);

            // Assert            
            Assert.Empty(matching.Data);
        }


        [Fact]
        public void GetMatchingRules_KeyValueCollectionMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { KeyValueCollection = new Dictionary<string, object> { { "DateOfBirth", DateTime.Now } } },
                [
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule
                                    {
                                        ComparisonOperator = Rule.ComparisonOperatorType.GreaterThan,
                                        ComparisonValue = DateTime.Now.AddSeconds(-2).ToString("o"),
                                        ComparisonPredicate = $"{nameof(TestModel.KeyValueCollection)}[DateOfBirth]",
                                        PredicateType = TypeCode.DateTime
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
        public void GetMatchingRules_KeyValueCollectionNotMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { KeyValueCollection = new Dictionary<string, object> { { "DateOfBirth", DateTime.Now } } },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    // PredicateType is needed here to be able to determine value type which is string
                                    new Rule
                                    {
                                        ComparisonOperator = Rule.ComparisonOperatorType.LessThan,
                                        ComparisonValue = DateTime.Now.AddMinutes(-5).ToString("o"),
                                        ComparisonPredicate = $"{nameof(TestModel.KeyValueCollection)}[DateOfBirth]" ,
                                        PredicateType = TypeCode.DateTime
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
        public void GetMatchingRules_EnumValueMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { SomeEnumValue = TestModel.SomeEnum.Yes },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.Equal, ComparisonValue = TestModel.SomeEnum.Yes.ToString(),  ComparisonPredicate = $"{nameof(TestModel.SomeEnumValue)}"}
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_PrimitiveInCollectionMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { NumericField = 3 },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.In, ComparisonValue ="1|2|3|4|5",  ComparisonPredicate = $"{nameof(TestModel.NumericField)}"}
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_PrimitiveNotInCollectionMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { NumericField = 10 },
                [
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.In, ComparisonValue ="1|2|3|4|5",  ComparisonPredicate = $"{nameof(TestModel.NumericField)}"}
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Empty(matching.Data);
        }


        [Fact]
        public void GetMatchingRules_MultiRulesAllMatch_AllRulesReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { NumericField = 10, TextField = "test1" },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.NotIn, ComparisonValue ="1|2|3|4|5",  ComparisonPredicate = $"{nameof(TestModel.NumericField)}"}
                                ]
                            }
                        ]
                    },
                     new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringEndsWith, ComparisonValue ="1",  ComparisonPredicate = $"{nameof(TestModel.TextField)}"}
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Equal(2, matching.Data.Count());
        }

        [Fact]
        public void GetMatchingRulesPrimitiveCollectionMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { PrimitivesCollection = [1, 2, 4, 5] },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.CollectionContainsAll, ComparisonValue ="1|2",  ComparisonPredicate = $"{nameof(TestModel.PrimitivesCollection)}"}
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }


        [Fact]
        public void GetMatchingRulesPrimitiveCollectionNoMatch_RuleNotReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService(), NullLogger.Instance);

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { PrimitivesCollection = [1, 2, 4, 5] },
                [
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.CollectionContainsAll, ComparisonValue ="1|2|10",  ComparisonPredicate = $"{nameof(TestModel.PrimitivesCollection)}"}
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Empty(matching.Data);
        }


        [Fact]
        public void GetMatchingRulesPrimitiveCollectionNoMatch_ComparisonPredicateNameAttribute_RuleReturned()
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
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringEqualsCaseInsensitive, ComparisonValue ="john",  ComparisonPredicate = "first_name"},
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.Equal, ComparisonValue ="123456789",  ComparisonPredicate = "userDetails[SSN]"},
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.Equal, ComparisonValue ="over the rainbow",  ComparisonPredicate = "userAddress.home_address"},
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.Equal, ComparisonValue ="1st",  ComparisonPredicate = "userAddress.StreetAddress"}
                                ]
                            }
                        ]
                    }
                ]);

            // Assert            
            Assert.Single(matching.Data);
        }


        [Fact]
        public void GetMatchingRulesP_FIrstMatchingRule_RuleReturned()
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
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringEqualsCaseInsensitive, ComparisonValue ="john",  ComparisonPredicate = "first_name"},
                                ]
                            }
                        ]
                    },
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = [
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.And,
                                Rules = [
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringContains, ComparisonValue ="jo",  ComparisonPredicate = "first_name"},
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
