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

        public virtual void SetInstance(T instance)
        {
            Instance = instance;
        }

        public virtual async Task<IRuleResult[]> ExecuteAsync()
        {
            ValidateInstance();

            if (Rules == null || !Rules.Any()) return _asyncRuleResults.ToArray();

            var parallelRuleResults = ExecuteParallelRules(Rules).ToList();

            var asynchronousRules = GetAsynchronousRules();

            foreach (var asyncRule in asynchronousRules)
            {
                AddToDataCollection(asyncRule);

                await asyncRule.BeforeInvokeAsync();

                if (!asyncRule.Skip && Constrained(asyncRule.Constraint))
                {
                    var ruleResult = await asyncRule.InvokeAsync(Instance);

                    AddToRuleResults(ruleResult, asyncRule.GetType().Name);
                }

                await asyncRule.AfterInvokeAsync();

                if (asyncRule.Terminate)
                {
                    break;
                }
            }
            if (parallelRuleResults.Any())
            {
                await Task.WhenAll(parallelRuleResults);
            }
                
            parallelRuleResults.ForEach(rule =>
            {
                var ruleResult = rule.Result;

                AddToRuleResults(ruleResult, rule.GetType().Name);
            });

            return _asyncRuleResults.ToArray();
        }

        private IEnumerable<IRuleAsync<T>> GetAsynchronousRules()
        {
            return Rules.OfType<IRuleAsync<T>>().Where(rule => !rule.Parallel);
        }

        public virtual IRuleResult[] Execute()
        {
            ValidateInstance();

            if (Rules != null && Rules.Any())
            {
                foreach (var rule in Rules.OfType<IRule<T>>())
                {
                    AddToDataCollection(rule);
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

                    if (rule.Terminate)
                    {
                        break;
                    }
                }
            }

            return _ruleResults.ToArray();
        }

        private IEnumerable<Task<IRuleResult>> ExecuteParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            var parallelRules = rules.OfType<IRuleAsync<T>>()
                .Where(rule => rule.Parallel)
                .ToList();

            if (!parallelRules.Any())
            {
                return Enumerable.Empty<Task<IRuleResult>>();
            }

            var parallelRuleResults = new List<Task<IRuleResult>>();

            if (parallelRules.Any())
            {
                parallelRules.ForEach(rule =>
                {
                    if (!rule.Skip && Constrained(rule.Constraint))
                    {
                        var parallelTask = Task.Run(() =>
                        {
                            return rule.BeforeInvokeAsync()
                                    .ContinueWith(t => rule.InvokeAsync(Instance).Result)
                                    .ContinueWith(t => { rule.AfterInvokeAsync(); return t.Result; });
                        });

                        parallelRuleResults.Add(parallelTask);
                    }
                });
            }
            
            return parallelRuleResults;
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

        private void AddToDataCollection(IGeneralRule<T> rule)
        {
            if (rule != null)
            {
                foreach (var pair in rule.Data)
                {
                    _data.TryAdd(pair.Key, pair.Value);
                }

                rule.Data = _data;
            }
        }

        private void AddToRuleResults(IRuleResult ruleResult, string ruleName)
        {
            if (ruleResult != null)
            {
                if (string.IsNullOrWhiteSpace(ruleResult.Name))
                {
                    ruleResult.Name = ruleResult.Name ?? ruleName;
                }

                _asyncRuleResults.Add(ruleResult);
            }
        }
    }
}