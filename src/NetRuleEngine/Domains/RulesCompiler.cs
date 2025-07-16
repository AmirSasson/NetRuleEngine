using NetRuleEngine.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NetRuleEngine.Domains
{
    public class RulesCompiler : IRulesCompiler
    {
        private static readonly ComparisonOperatorType[] CollectionComparisonTypes = new ComparisonOperatorType[] { ComparisonOperatorType.CollectionNotContainsAnyOf, ComparisonOperatorType.CollectionContainsAll, ComparisonOperatorType.CollectionContainsAnyOf };

        ///
        /// A method used to precompile rules for a provided type
        ///
        public (Func<T, bool> CompliedRule, string RuleDescription) CompileRule<T>(RulesConfig rulesConfig)
        {
            // Loop through the rules and compile them against the properties of the supplied shallow object
            Expression combinedExp = null;
            var genericType = Expression.Parameter(typeof(T));

            foreach (var ruleNode in rulesConfig.RulesGroups)
            {
                var groupExpression = ProcessRuleOrGroup<T>(genericType, ruleNode);

                // Stitching the rules into 1 statement
                if (rulesConfig.RulesOperator == InternalRuleOperatorType.And)
                {
                    combinedExp = combinedExp != null ? Expression.AndAlso(combinedExp, groupExpression) : groupExpression;
                }
                else // OR
                {
                    combinedExp = combinedExp != null ? Expression.OrElse(combinedExp, groupExpression) : groupExpression;
                }
            }
            return (CompliedRule: Expression.Lambda<Func<T, bool>>(combinedExp ?? Expression.Constant(true), genericType).Compile(), RuleDescription: combinedExp?.ToString() ?? "Empty rule");
        }

        private Expression ProcessRuleOrGroup<T>(ParameterExpression genericType, RuleNode ruleNode)
        {
            // If it's a group, process its rules recursively
            if (ruleNode is RulesGroup group)
            {
                Expression groupExp = null;
                foreach (var childRule in group.Rules)
                {
                    var childExp = ProcessRuleOrGroup<T>(genericType, childRule);

                    // Combine with group operator
                    if (group.Operator == InternalRuleOperatorType.And)
                    {
                        groupExp = groupExp != null ? Expression.AndAlso(groupExp, childExp) : childExp;
                    }
                    else // OR
                    {
                        groupExp = groupExp != null ? Expression.OrElse(groupExp, childExp) : childExp;
                    }
                }
                return groupExp ?? Expression.Constant(true);
            }

            // If it's a simple rule, use existing logic
            return getRuleExpression<T>(genericType, ruleNode as Rule);
        }

        private static Expression getRuleExpression<T>(ParameterExpression genericType, Rule rule)
        {
            Expression binaryExpression;

            // Handle Nested Property (i.e. for some object that has property with name Composite that has a property of name NumericField  => Composite.NumericField)
            if (rule.ComparisonPredicate.Contains("."))
            {
                var tokens = rule.ComparisonPredicate.Split('.');

                var parentRulePropertyName = tokens[0];
                var parentPropertyName = customRulePropertyToTypePropertyName(genericType.Type, parentRulePropertyName);

                var parentPropExpression = MemberExpression.Property(genericType, parentPropertyName);

                var nestedPropertyRulePropertyName = tokens[1];
                var nestedPropertyPropertyName = customRulePropertyToTypePropertyName(parentPropExpression.Type, nestedPropertyRulePropertyName);

                var nestedPropertyFullMemberExpression = MemberExpression.Property(parentPropExpression, nestedPropertyPropertyName);
                var propertyType = nestedPropertyFullMemberExpression.Type;
                var nullCheck = Expression.NotEqual(parentPropExpression, Expression.Constant(null, typeof(object)));

                // Collection
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(nestedPropertyFullMemberExpression.Type) && CollectionComparisonTypes.Contains(rule.ComparisonOperator))
                {
                    binaryExpression = createCollectionExpression(rule, nestedPropertyFullMemberExpression, nestedPropertyFullMemberExpression.Type);
                }
                else
                {
                    binaryExpression = createSingleExpression(rule, nestedPropertyFullMemberExpression, propertyType);
                }

                binaryExpression = Expression.AndAlso(nullCheck, binaryExpression);
            }
            else if (rule.ComparisonPredicate.Contains("[")) // Dictionary type
            {
                var dicPropRuleName = rule.ComparisonPredicate.Split('[').First();
                var dicPropName = customRulePropertyToTypePropertyName(genericType.Type, dicPropRuleName);

                var key = MemberExpression.Property(genericType, dicPropName);
                var propertyType = typeof(T).GetProperty(dicPropName).PropertyType;

                var indexName = rule.ComparisonPredicate.Split('[').Last().Trim(']');
                System.Reflection.PropertyInfo indexer = propertyType.GetProperty("Item");
                IndexExpression indexExpr = Expression.Property(key, indexer, Expression.Constant(indexName));
                Type valueProperty = propertyType.GenericTypeArguments.Last();
                Expression convertedIndexer = null;
                if (rule.PredicateType.HasValue)
                {
                    TypeCode typeCode = rule.PredicateType.Value;
                    var changeTypeCall = Expression.Call(typeof(Convert).GetMethod("ChangeType",
                                                           new[] { typeof(object),
                                                            typeof(TypeCode) }),
                                                            indexExpr,
                                                            Expression.Constant(typeCode)
                                                            );
                    valueProperty = Type.GetType("System." + typeCode);
                    convertedIndexer = Expression.Convert(changeTypeCall, valueProperty);
                }

                System.Reflection.MethodInfo mi = propertyType.GetMethod("ContainsKey");
                var containsKeyExpression = Expression.Call(key, mi, Expression.Constant(indexName));
                var operand = createSingleExpression(rule, convertedIndexer ?? indexExpr, valueProperty);
                binaryExpression = Expression.AndAlso(containsKeyExpression, operand);
            }
            else
            {
                var comparisonPredicate = customRulePropertyToTypePropertyName(genericType.Type, rule.ComparisonPredicate);

                var key = MemberExpression.Property(genericType, comparisonPredicate);
                var propertyType = typeof(T).GetProperty(comparisonPredicate).PropertyType;

                // Collection
                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propertyType) && CollectionComparisonTypes.Contains(rule.ComparisonOperator))
                {
                    binaryExpression = createCollectionExpression(rule, key, propertyType);
                }
                else
                {
                    binaryExpression = createSingleExpression(rule, key, propertyType);
                }
            }

            return binaryExpression;
        }

        // translating ComparisonPredicate to the property name by ComparisonPredicateNameAttribute if exists.
        static string customRulePropertyToTypePropertyName(Type objectType, string rulePropertyName)
        {
            var propertyByNameAttribute = objectType.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<RulePredicatePropertyAttribute>()?.Name == rulePropertyName);
            if (propertyByNameAttribute != null)
            {
                return propertyByNameAttribute.Name;
            }

            return rulePropertyName;
        }

        private static Expression createSingleExpression(Rule rule, Expression key, Type propertyType)
        {
            Expression binaryExpression;

            Lazy<Expression> notNullCheck = new Lazy<Expression>(() => Expression.NotEqual(key, Expression.Constant(null, typeof(object))));

            if (rule.ComparisonOperator == ComparisonOperatorType.In)
            {
                var arrValue = rule.ComparisonValue.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                IEnumerable<Expression> trees;
                if (propertyType.IsEnum)
                {
                    trees = arrValue.Select(a => Expression.Constant(Enum.Parse(propertyType, a)));
                }
                else
                {
                    trees = arrValue.Select(a => Expression.Constant(Convert.ChangeType(a, propertyType)));
                }
                NewArrayExpression newArrayExpression = Expression.NewArrayInit(propertyType, trees);
                var callContains = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), new Type[] { propertyType }, newArrayExpression, key);
                return callContains;

            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.NotIn)
            {
                var arrValue = rule.ComparisonValue.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                IEnumerable<Expression> trees;
                if (propertyType.IsEnum)
                {
                    trees = arrValue.Select(a => Expression.Constant(Enum.Parse(propertyType, a)));
                }
                else
                {
                    trees = arrValue.Select(a => Expression.Constant(Convert.ChangeType(a, propertyType)));
                }
                NewArrayExpression newArrayExpression = Expression.NewArrayInit(propertyType, trees);
                var callContains = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), new Type[] { propertyType }, newArrayExpression, key);
                return Expression.Not(callContains);
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.IsTrue)
            {
                binaryExpression = Expression.IsTrue(key);
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.IsFalse)
            {
                binaryExpression = Expression.IsFalse(key);
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.StringEqualsCaseInsensitive)
            {
                System.Reflection.MethodInfo mi = typeof(string).GetMethod(nameof(String.Equals), new Type[] { typeof(string), typeof(StringComparison) });
                var call = Expression.Call(key, mi, Expression.Constant(rule.ComparisonValue), Expression.Constant(StringComparison.InvariantCultureIgnoreCase));
                binaryExpression = Expression.AndAlso(notNullCheck.Value, call);
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.StringNotEqualsCaseInsensitive)
            {
                System.Reflection.MethodInfo mi = typeof(string).GetMethod(nameof(String.Equals), new Type[] { typeof(string), typeof(StringComparison) });
                var call = Expression.Not(Expression.Call(key, mi, Expression.Constant(rule.ComparisonValue), Expression.Constant(StringComparison.InvariantCultureIgnoreCase)));
                binaryExpression = Expression.AndAlso(notNullCheck.Value, call);
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.StringNullOrEmpty)
            {
                System.Reflection.MethodInfo mi = typeof(String).GetMethod(nameof(String.IsNullOrWhiteSpace), bindingAttr: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, types: new[] { typeof(string) }, binder: null, modifiers: null);
                var call = Expression.Call(mi, key);
                binaryExpression = call;
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.StringNotNullOrEmpty)
            {
                System.Reflection.MethodInfo mi = typeof(String).GetMethod(nameof(String.IsNullOrWhiteSpace), bindingAttr: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, types: new[] { typeof(string) }, binder: null, modifiers: null);
                var call = Expression.Not(Expression.Call(mi, key));
                binaryExpression = call;
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.StringStartsWith)
            {
                System.Reflection.MethodInfo mi = typeof(string).GetMethod(nameof(String.StartsWith), new Type[] { typeof(string), typeof(StringComparison) });
                var call = Expression.Call(key, mi, Expression.Constant(rule.ComparisonValue), Expression.Constant(StringComparison.InvariantCultureIgnoreCase));
                binaryExpression = Expression.AndAlso(notNullCheck.Value, call);
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.StringEndsWith)
            {
                System.Reflection.MethodInfo mi = typeof(string).GetMethod(nameof(String.EndsWith), new Type[] { typeof(string), typeof(StringComparison) });
                var call = Expression.Call(key, mi, Expression.Constant(rule.ComparisonValue), Expression.Constant(StringComparison.InvariantCultureIgnoreCase));
                binaryExpression = Expression.AndAlso(notNullCheck.Value, call);
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.StringContains)
            {
                System.Reflection.MethodInfo mi = typeof(string).GetMethod(nameof(String.Contains), new Type[] { typeof(string), typeof(StringComparison) });
                var call = Expression.Call(key, mi, Expression.Constant(rule.ComparisonValue), Expression.Constant(StringComparison.InvariantCultureIgnoreCase));
                binaryExpression = Expression.AndAlso(notNullCheck.Value, call);
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.StringNotContains)
            {
                System.Reflection.MethodInfo mi = typeof(string).GetMethod(nameof(String.Contains), new Type[] { typeof(string), typeof(StringComparison) });
                var call = Expression.Not(Expression.Call(key, mi, Expression.Constant(rule.ComparisonValue), Expression.Constant(StringComparison.InvariantCultureIgnoreCase)));
                binaryExpression = Expression.AndAlso(notNullCheck.Value, call);
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.StringMatchesRegex)
            {
                System.Reflection.MethodInfo mi = typeof(Regex).GetMethod("IsMatch", bindingAttr: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, types: new[] { typeof(string), typeof(string) }, binder: null, modifiers: null);
                var call = Expression.Call(mi, key, Expression.Constant(rule.ComparisonValue));
                binaryExpression = Expression.AndAlso(notNullCheck.Value, call);
            }
            else
            {
                object typedValue;
                if (propertyType.IsEnum)
                {
                    typedValue = Enum.Parse(propertyType, rule.ComparisonValue);
                }
                else
                {
                    typedValue = Convert.ChangeType(rule.ComparisonValue, propertyType.IsGenericType ? propertyType.GenericTypeArguments[0] : propertyType);
                }
                var value = Expression.Convert(Expression.Constant(typedValue), propertyType);

                binaryExpression = Expression.MakeBinary((ExpressionType)rule.ComparisonOperator, key, value);
            }

            return binaryExpression;
        }

        private static Expression createCollectionExpression(Rule rule, MemberExpression key, Type propertyType)
        {
            Expression binaryExpression = null;
            var notNullCheck = Expression.NotEqual(key, Expression.Constant(null, typeof(object)));
            var isNullCheck = Expression.Equal(key, Expression.Constant(null, typeof(object)));
            var elementType = propertyType.IsGenericType ? propertyType.GenericTypeArguments[0] : propertyType.GetElementType();
            var arrValue = rule.ComparisonValue.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<Expression> trees = arrValue.Select(a => Expression.Constant(Convert.ChangeType(a, elementType)));
            NewArrayExpression newArrayExpression = Expression.NewArrayInit(elementType, trees);
            var callIntersect = Expression.Call(typeof(Enumerable), nameof(Enumerable.Intersect), new Type[] { elementType }, key, newArrayExpression);

            if (rule.ComparisonOperator == ComparisonOperatorType.CollectionContainsAnyOf) // intersection.Any()
            {
                var callAny = Expression.Call(typeof(Enumerable), nameof(Enumerable.Any), new Type[] { elementType }, callIntersect);

                binaryExpression = Expression.AndAlso(notNullCheck, callAny);
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.CollectionNotContainsAnyOf) // !intersection.Any()
            {
                var callAny = Expression.Call(typeof(Enumerable), nameof(Enumerable.Any), new Type[] { elementType }, callIntersect);

                binaryExpression = Expression.AndAlso(notNullCheck, Expression.Not(callAny));
            }
            else if (rule.ComparisonOperator == ComparisonOperatorType.CollectionContainsAll) // intersection.Count() == value.Count()
            {
                var callCount = Expression.Call(typeof(Enumerable), nameof(Enumerable.Count), new Type[] { elementType }, callIntersect);
                binaryExpression = Expression.AndAlso(notNullCheck, Expression.Equal(callCount, Expression.Constant(arrValue.Length, typeof(int))));
            }

            return binaryExpression;
        }
    }
}
