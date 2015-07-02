using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DotNetRuleEngine.Core
{
    public abstract class RuleEngine<T> where T : class, new()
    {
        private readonly ConcurrentDictionary<string, object> _data = new ConcurrentDictionary<string, object>();
        private readonly IList<IRuleResult> _ruleResults = new List<IRuleResult>();
        private readonly IList<IRuleResult> _asyncRuleResults = new List<IRuleResult>();

        protected RuleEngine()
        {
            Instance = this as T;
        }

        public T Instance { get; set; }

        protected IList<IGeneralRule<T>> Rules { get; private set; }

        public virtual void AddRules(params IGeneralRule<T>[] rules)
        {
            Rules = rules.ToList();
        }

        public virtual async Task<IRuleResult[]> ExecuteAsync()
        {
            ValidateInstance();

            if (Rules != null && Rules.Any())
            {
                foreach (var asyncRule in Rules.OfType<IRuleAsync<T>>())
                {
                    AddRuleAsyncDataValues(asyncRule);
                    await asyncRule.BeforeInvokeAsync();

                    if (!asyncRule.Skip && Constrained(asyncRule.Constraint))
                    {
                        var ruleResult = await asyncRule.InvokeAsync(Instance);
                        if (ruleResult != null)
                        {
                            if (string.IsNullOrWhiteSpace(ruleResult.Name))
                            {
                                ruleResult.Name = asyncRule.GetType().Name;
                            }
                            _asyncRuleResults.Add(ruleResult);
                        }
                    }

                    await asyncRule.AfterInvokeAsync();
                    AddRuleAsyncDataValues(asyncRule);

                    if (asyncRule.Terminate)
                    {
                        break;
                    }
                }
            }

            return _asyncRuleResults.ToArray();
        }

        public virtual IRuleResult[] Execute()
        {
            ValidateInstance();

            if (Rules != null && Rules.Any())
            {
                foreach (var rule in Rules.OfType<IRule<T>>())
                {
                    AddRuleDataValues(rule);
                    rule.BeforeInvoke();

                    if (!rule.Skip && Constrained(rule.Constraint))
                    {
                        var ruleResult = rule.Invoke(Instance);
                        if (ruleResult != null)
                        {
                            if (string.IsNullOrWhiteSpace(ruleResult.Name))
                            {
                                ruleResult.Name = rule.GetType().Name;
                            }
                            _ruleResults.Add(ruleResult);
                        }
                    }

                    rule.AfterInvoke();
                    AddRuleDataValues(rule);

                    if (rule.Terminate)
                    {
                        break;
                    }
                }
            }

            return _ruleResults.ToArray();
        }

        private void AddRuleDataValues(IGeneralRule<T> rule)
        {
            AddToDataCollection(rule);
        }

        private void AddRuleAsyncDataValues(IGeneralRule<T> rule)
        {
            AddToDataCollection(rule);
        }

        private void ValidateInstance()
        {
            if (Instance == null)
            {
                throw new InvalidOperationException("Instance not set");
            }
        }

        private bool Constrained(Expression<Predicate<T>> predicate)
        {
            return predicate == null || predicate.Compile().Invoke(Instance);
        }

        private void AddToDataCollection(IGeneralRule<T> tmp)
        {
            if (tmp != null)
            {
                foreach (var pair in tmp.Data)
                {
                    _data.TryAdd(pair.Key, pair.Value);
                }
                tmp.Data = _data;
            }
        }
    }
}