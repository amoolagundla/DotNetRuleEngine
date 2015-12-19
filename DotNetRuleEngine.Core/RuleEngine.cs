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
        private readonly ConcurrentDictionary<string, object> _sharedData = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, Task<object>> _sharedDataAsync = new ConcurrentDictionary<string, Task<object>>();
        private readonly IList<IRuleResult> _ruleResults = new List<IRuleResult>();
        private readonly IList<IRuleResult> _asyncRuleResults = new List<IRuleResult>();

        protected RuleEngine()
        {
            Instance = this as T;
        }

        protected IList<IGeneralRule<T>> Rules { get; private set; }

        public T Instance { get; set; }


        public virtual RuleEngine<T> AddRules(params IGeneralRule<T>[] rules)
        {
            Rules = rules.ToList();
            return this;
        }

        public virtual void SetInstance(T instance)
        {
            Instance = instance;
        }

        public virtual async Task<IRuleResult[]> ExecuteAsync()
        {
            ValidateInstance();

            if (Rules == null || !Rules.Any()) return _asyncRuleResults.ToArray();

            InitializeExecutionOrder();

            var rulesWithExecutionOrder = GetRulesWithExecutionOrder<IRuleAsync<T>>(r=> r.ExecutionOrder.HasValue);
            var rulesWithoutExecutionOrder = GetRulesWithoutExecutionOrder<IRuleAsync<T>>(r => !r.Parallel && !r.ExecutionOrder.HasValue);
            var asynchronousRules = rulesWithExecutionOrder.Concat(rulesWithoutExecutionOrder);

            var parallelRuleResults = ExecuteParallelRules(Rules).ToList();

            foreach (var asyncRule in asynchronousRules)
            {
                AddToAsyncDataCollection(asyncRule);

                await asyncRule.BeforeInvokeAsync();

                if (!asyncRule.Skip && Constrained(asyncRule.Constraint))
                {
                    var ruleResult = await asyncRule.InvokeAsync(Instance);

                    AddToRuleResults(ruleResult, asyncRule.GetType().Name, _asyncRuleResults);
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

                AddToRuleResults(ruleResult, rule.GetType().Name, _asyncRuleResults);
            });

            return _asyncRuleResults.ToArray();
        }

        public virtual IRuleResult[] Execute()
        {
            ValidateInstance();

            if (Rules == null || !Rules.Any()) return _ruleResults.ToArray();

            InitializeExecutionOrder();

            var rulesWithExecutionOrder = GetRulesWithExecutionOrder<IRule<T>>();
            var rulesWithoutExecutionOrder = GetRulesWithoutExecutionOrder<IRule<T>>();
            var rules = rulesWithExecutionOrder.Concat(rulesWithoutExecutionOrder);

            foreach (var rule in rules)
            {
                AddToDataCollection(rule);
                rule.BeforeInvoke();

                if (!rule.Skip && Constrained(rule.Constraint))
                {
                    var ruleResult = rule.Invoke(Instance);

                    AddToRuleResults(ruleResult, rule.GetType().Name, _ruleResults);                        
                }

                rule.AfterInvoke();

                if (rule.Terminate)
                {
                    break;
                }
            }

            return _ruleResults.ToArray();
        }        

        private IEnumerable<Task<IRuleResult>> ExecuteParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            var parallelRules = rules.OfType<IRuleAsync<T>>()
                .Where(rule => rule.Parallel && !rule.ExecutionOrder.HasValue)
                .ToList();

            if (!parallelRules.Any())
            {
                return Enumerable.Empty<Task<IRuleResult>>();
            }

            var parallelRuleResults = new List<Task<IRuleResult>>();

            if (parallelRules.Any())
            {
                parallelRules.ForEach(pRule =>
                {
                    AddToAsyncDataCollection(pRule);

                    if (!pRule.Skip && Constrained(pRule.Constraint))
                    {
                        var parallelTask = Task.Run(() =>
                        {
                            return pRule.BeforeInvokeAsync()
                                    .ContinueWith(t => pRule.InvokeAsync(Instance).Result)
                                    .ContinueWith(t => { pRule.AfterInvokeAsync(); return t.Result; });
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

        private void AddToAsyncDataCollection(IRuleAsync<T> rule)
        {
            if (rule != null)
            {
                foreach (var pair in rule.Data)
                {
                    _sharedDataAsync.TryAdd(pair.Key, pair.Value);
                }

                rule.Data = _sharedDataAsync;
            }
        }

        private void AddToDataCollection(IRule<T> rule)
        {
            if (rule != null)
            {
                foreach (var pair in rule.Data)
                {
                    _sharedData.TryAdd(pair.Key, pair.Value);
                }

                rule.Data = _sharedData;
            }
        }

        private void AddToRuleResults(IRuleResult ruleResult, string ruleName, 
            ICollection<IRuleResult> ruleResults)
        {
            if (ruleResult != null)
            {
                ruleResult.Name = ruleResult.Name ?? ruleName;

                ruleResults.Add(ruleResult);
            }
        }

        private IEnumerable<TK> GetRulesWithoutExecutionOrder<TK>(Func<TK, bool> condition = null) 
            where TK : IGeneralRule<T>
        {
            condition = condition ?? (k => true);

            return Rules.OfType<TK>().Where(r => !r.ExecutionOrder.HasValue)
                .Where(condition).ToList();
        }

        private IEnumerable<TK> GetRulesWithExecutionOrder<TK>(Func<TK, bool> condition = null) 
            where TK : IGeneralRule<T>
        {
            condition = condition ?? (k => true);

            return Rules.OfType<TK>()
                .Where(r => r.ExecutionOrder.HasValue)
                .Where(condition)
                .OrderBy(r => r.ExecutionOrder)
                .ToList();
        }

        private void InitializeExecutionOrder()
        {
            foreach (var rule in Rules)
            {
                rule.SetExecutionOrder();
            }
        }
    }
}