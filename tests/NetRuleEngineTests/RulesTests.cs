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
        [Theory]
        [InlineData(5, Rule.ComparisonOperatorType.Equal, 5, true)]
        [InlineData(5, Rule.ComparisonOperatorType.LessThan, 4, true)]
        [InlineData(5, Rule.ComparisonOperatorType.LessThan, 6, false)]
        [InlineData(5, Rule.ComparisonOperatorType.GreaterThan, 6, true)]
        public void GetMatchingRules_NumericValueMatch_ShouldMatchByOperatorAndValue(int ruleVal, Rule.ComparisonOperatorType op, int objectVal, bool shouldMatch)
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var numericValueTest = objectVal;
            var matching = engine.GetMatchingRules(
                new TestModel { NumericField = numericValueTest },
                new[] {
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.And,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = op, ComparisonValue = ruleVal.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                }
                            }
                        }
                    }
                });

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
                new[] {
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.And,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.Equal, ComparisonValue = numericValueOtherValue.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                }
                            }
                        }
                    }
                });

            // Assert            
            Assert.Empty(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_StringStartsWithMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "SomePrefixBlahBlah" },
                new[] {
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.And,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "someprefix",  ComparisonPredicate = nameof(TestModel.TextField) }
                                }
                            }
                        }
                    }
                });

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_MultiRuleAndMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "SomePrefixBlahBlah", NumericField = 10 },
                new[] {
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.And,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "someprefix",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThan, ComparisonValue = 4.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                }
                            }
                        }
                    }
                });

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_MultiRuleOrMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "SomePrefixBlahBlah", NumericField = 10 },
                new[] {
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "NOT MATCHING PREFIX",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThan, ComparisonValue = 4.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                }
                            }
                        }
                    }
                });

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_MultiGroupAnMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var ruleConfig =
                    new RulesConfig
                    {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "NOT MATCHING PREFIX",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThan, ComparisonValue = 4.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                }
                            },
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "SomePrefix",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThan, ComparisonValue = 55.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                }
                            }
                        }
                    };
            var text = ruleConfig.ToJson();
            var deserializedRules = FromJson(text);

            var matching = engine.GetMatchingRules(
                    new TestModel { TextField = "SomePrefixBlahBlah", NumericField = 10 },
                    new[] { deserializedRules });
            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_MultiGroupFirstGroupNotMatch_RuleNotReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { TextField = "SomePrefixBlahBlah", NumericField = 10 },
                new[] {
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {                // this group does not match!
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "NOT MATCHING PREFIX",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.LessThan, ComparisonValue = 4.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                }
                            },
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringStartsWith, ComparisonValue = "SomePrefix",  ComparisonPredicate = nameof(TestModel.TextField) },
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThan, ComparisonValue = 55.ToString(),  ComparisonPredicate = nameof(TestModel.NumericField) }
                                }
                            }
                        }
                    }
                });

            // Assert            
            Assert.Empty(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_CompositePropertyMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { Composit = new TestModel.CompositeInnerClass { NumericField = 10 } },
                new[] {
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThanOrEqual, ComparisonValue = 4.ToString(),  ComparisonPredicate = $"{nameof(TestModel.Composit)}.{nameof(TestModel.Composit.NumericField)}"}
                            }
                        }
                    }
                    }
                });

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_CaluculatedCOllectionCollectionContainsAnyOfMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { CompositeCollection = new List<TestModel.CompositeInnerClass> { new TestModel.CompositeInnerClass { NumericField = 10 } } },
                new[] {
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.CollectionContainsAnyOf, ComparisonValue = "10|11|12",  ComparisonPredicate = $"{nameof(TestModel.CaluculatedCollection)}"}
                            }
                        }
                    }
                    }
                });

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_CaluculatedCollectionNotContainsAnyOfNotMatch_RuleNotReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { CompositeCollection = new List<TestModel.CompositeInnerClass> { new TestModel.CompositeInnerClass { NumericField = 10 } } },
                new[] {
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.CollectionNotContainsAnyOf, ComparisonValue = "10|11|12",  ComparisonPredicate = $"{nameof(TestModel.CaluculatedCollection)}"}
                            }
                        }
                    }
                    }
                });

            // Assert            
            Assert.Empty(matching.Data);
        }


        [Fact]
        public void GetMatchingRules_KeyValueCollectionMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { KeyValueCollection = new Dictionary<string, object> { { "DateOfBirth", DateTime.Now } } },
                new[] {
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.GreaterThan, ComparisonValue = DateTime.Now.AddSeconds(-2).ToString("U"),  ComparisonPredicate = $"{nameof(TestModel.KeyValueCollection)}[DateOfBirth]" , PredicateType = TypeCode.DateTime}
                            }
                        }
                    }
                    }
                });

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_KeyValueCollectionNotMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { KeyValueCollection = new Dictionary<string, object> { { "DateOfBirth", DateTime.Now } } },
                new[] {
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    // PredicateType is needed here to be able to determine value type which is string
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.LessThan, ComparisonValue = DateTime.Now.AddSeconds(-2).ToString("U"),  ComparisonPredicate = $"{nameof(TestModel.KeyValueCollection)}[DateOfBirth]" , PredicateType = TypeCode.DateTime}
                            }
                        }
                    }
                    }
                });

            // Assert            
            Assert.Empty(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_EnumValueMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { SomeEnumValue = TestModel.SomeEnum.Yes },
                new[] {
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.Equal, ComparisonValue = TestModel.SomeEnum.Yes.ToString(),  ComparisonPredicate = $"{nameof(TestModel.SomeEnumValue)}"}
                                }
                            }
                        }
                    }
                });

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_PrimitiveInCollectionMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { NumericField = 3 },
                new[] {
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.In, ComparisonValue ="1|2|3|4|5",  ComparisonPredicate = $"{nameof(TestModel.NumericField)}"}
                                }
                            }
                        }
                    }
                });

            // Assert            
            Assert.Single(matching.Data);
        }

        [Fact]
        public void GetMatchingRules_PrimitiveNotInCollectionMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { NumericField = 10 },
                new[] {
                    new RulesConfig {
                         Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.In, ComparisonValue ="1|2|3|4|5",  ComparisonPredicate = $"{nameof(TestModel.NumericField)}"}
                                }
                            }
                        }
                    }
                });

            // Assert            
            Assert.Empty(matching.Data);
        }


        [Fact]
        public void GetMatchingRules_MultiRulesAllMatch_AllRulesReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { NumericField = 10, TextField = "test1" },
                new[] {
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.NotIn, ComparisonValue ="1|2|3|4|5",  ComparisonPredicate = $"{nameof(TestModel.NumericField)}"}
                                }
                            }
                        }
                    },
                     new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.StringEndsWith, ComparisonValue ="1",  ComparisonPredicate = $"{nameof(TestModel.TextField)}"}
                                }
                            }
                        }
                    }
                });

            // Assert            
            Assert.Equal(2, matching.Data.Count());
        }

        [Fact]
        public void GetMatchingRulesPrimitiveCollectionMatch_RuleReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { PrimitivesCollection = new List<int> { 1, 2, 4, 5 } },
                new[] {
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.CollectionContainsAll, ComparisonValue ="1|2",  ComparisonPredicate = $"{nameof(TestModel.PrimitivesCollection)}"}
                                }
                            }
                        }
                    }
                });

            // Assert            
            Assert.Single(matching.Data);
        }


        [Fact]
        public void GetMatchingRulesPrimitiveCollectionNoMatch_RuleNotReturned()
        {
            // Arrange
            var engine = new RulesService<TestModel>(new RulesCompiler(), new LazyCache.Mocks.MockCachingService());

            // Act
            var matching = engine.GetMatchingRules(
                new TestModel { PrimitivesCollection = new List<int> { 1, 2, 4, 5 } },
                new[] {
                    new RulesConfig {
                        Id = Guid.NewGuid(),
                        RulesOperator = Rule.InterRuleOperatorType.And,
                        RulesGroups = new RulesGroup[] {
                            new RulesGroup {
                                RulesOperator = Rule.InterRuleOperatorType.Or,
                                Rules = new[] {
                                    new Rule { ComparisonOperator = Rule.ComparisonOperatorType.CollectionContainsAll, ComparisonValue ="1|2|10",  ComparisonPredicate = $"{nameof(TestModel.PrimitivesCollection)}"}
                                }
                            }
                        }
                    }
                });

            // Assert            
            Assert.Empty(matching.Data);
        }
    }
}
