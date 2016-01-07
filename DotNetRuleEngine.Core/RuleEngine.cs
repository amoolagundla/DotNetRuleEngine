using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core
{
    /// <summary>
    /// Rule Engine.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RuleEngine<T> where T : class, new()
    {
        private readonly ConcurrentDictionary<string, object> _sharedData = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, Task<object>> _sharedDataAsync = new ConcurrentDictionary<string, Task<object>>();
        private readonly ICollection<IRuleResult> _ruleResults = new List<IRuleResult>();
        private readonly ICollection<IRuleResult> _asyncRuleResults = new List<IRuleResult>();
        private readonly ICollection<Task<IRuleResult>> _parallelRuleResults = new List<Task<IRuleResult>>();

        /// <summary>
        /// Rule engine ctor.
        /// </summary>
        protected RuleEngine()
        {
            Instance = this as T;
        }

        /// <summary>
        /// Rules.
        /// </summary>
        protected ICollection<IGeneralRule<T>> Rules { get; private set; }

        /// <summary>
        /// Instance
        /// </summary>
        public T Instance { get; set; }

        /// <summary>
        /// Used to add rules to rule engine.
        /// </summary>
        /// <param name="rules">Rule(s) list.</param>
        public virtual void AddRules(params IGeneralRule<T>[] rules)
        {
            Rules = rules.ToList();           
        }

        /// <summary>
        /// Used to set instance.
        /// </summary>
        /// <param name="instance">Instance</param>
        public virtual void SetInstance(T instance)
        {
            Instance = instance;
        }

        /// <summary>
        /// Used to execute async rules.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IRuleResult[]> ExecuteAsync()
        {
            ValidateInstance();

            if (Rules == null || !Rules.Any()) return _asyncRuleResults.ToArray();

            Initialize(Rules);

            await ExecuteAsyncRules(Rules.OfType<IRuleAsync<T>>().ToList());

            if (_parallelRuleResults.Any())
            {
                await Task.WhenAll(_parallelRuleResults);
            }

            _parallelRuleResults.ToList().ForEach(rule =>
            {
                AddToAsyncRuleResults(rule.Result, rule.GetType().Name);
            });

            return _asyncRuleResults.ToArray();
        }

        /// <summary>
        /// Used to execute rules.
        /// </summary>
        /// <returns></returns>
        public virtual IRuleResult[] Execute()
        {
            ValidateInstance();

            if (Rules == null || !Rules.Any()) return _ruleResults.ToArray();

            Initialize(Rules);
            var rules = OrderByExecutionOrder(Rules);

            foreach (var rule in rules)
            {
                if (Execute(rule)) break;
            }

            return _ruleResults.ToArray();
        }

        private async Task ExecuteAsyncRules(IReadOnlyCollection<IGeneralRule<T>> rules)
        {
            ExecuteParallelRules(rules);

            var asynRules = GetAsyncRules(rules);

            foreach (var asyncRule in asynRules)
            {
                SynchronizeAsyncDataCollection(asyncRule);

                if (!asyncRule.Configuration.Skip && Constrained(asyncRule.Configuration.Constraint))
                {                    
                    if (asyncRule.IsNested)
                    {
                        await ExecuteAsyncRules(asyncRule.GetRules());
                    }

                    await asyncRule.BeforeInvokeAsync();

                    var ruleResult = await asyncRule.InvokeAsync(Instance);

                    AddToAsyncRuleResults(ruleResult, asyncRule.GetType().Name);

                    await asyncRule.AfterInvokeAsync();

                    SynchronizeAsyncDataCollection(asyncRule);
                }

                if (asyncRule.Configuration.Terminate)
                {
                    break;
                }
            }
        }

        private IEnumerable<IRuleAsync<T>> GetAsyncRules(IEnumerable<IGeneralRule<T>> rules)
        {
            var asyncRules = rules.OfType<IRuleAsync<T>>()
                .Where(rule => !rule.Parallel)
                .ToList();

            OrderByAsyncRuleExecutionOrder(asyncRules);

            return asyncRules;
        }

        private IEnumerable<IRuleAsync<T>> GetParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            var parallelRules = rules.OfType<IRuleAsync<T>>()
                 .Where(rule => rule.Parallel &&
                 !rule.Configuration.ExecutionOrder.HasValue).ToList();

            return parallelRules;
        }

        private bool Execute(IRule<T> rule)
        {
            if (!rule.Configuration.Skip && Constrained(rule.Configuration.Constraint))
            {
                SynchronizeDataCollection(rule);

                if (rule.IsNested)
                {
                    var rules = OrderByExecutionOrder(rule.GetRules());

                    foreach (var childRule in rules)
                    {
                        Execute(childRule);
                    }
                }

                rule.BeforeInvoke();

                var ruleResult = rule.Invoke(Instance);
                AddToRuleResults(ruleResult, rule.GetType().Name);
            }

            rule.AfterInvoke();
            SynchronizeDataCollection(rule);

            return rule.Configuration.Terminate;
        }

        private void ExecuteParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            var parallelRules = GetParallelRules(rules).ToList();

            if (parallelRules.Any()) ExecuteParallelRule(parallelRules, _parallelRuleResults);
        }

        private void ExecuteParallelRule(IEnumerable<IRuleAsync<T>> parallelRules,
            ICollection<Task<IRuleResult>> parallelRuleResults)
        {
            foreach (var pRule in parallelRules)
            {
                if (!pRule.Configuration.Skip && Constrained(pRule.Configuration.Constraint))
                {
                    SynchronizeAsyncDataCollection(pRule);

                    if (pRule.IsNested)
                    {
                        var parallelNestedRules = GetParallelRules(pRule.GetRules());
                        ExecuteParallelRule(parallelNestedRules, parallelRuleResults);
                    }

                    var parallelTask = Task.Run(async () =>
                    {                        
                        await pRule.BeforeInvokeAsync();
                        var ruleResult = await pRule.InvokeAsync(Instance);
                        
                        await pRule.AfterInvokeAsync();

                        SynchronizeAsyncDataCollection(pRule);

                        return ruleResult;
                    });

                    parallelRuleResults.Add(parallelTask);
                }
            }
        }

        private void OrderByAsyncRuleExecutionOrder(ICollection<IRuleAsync<T>> rules)
        {
            var rulesWithExecutionOrder =
                GetRulesWithExecutionOrder<IRuleAsync<T>>(rules, r => r.Configuration.ExecutionOrder.HasValue);

            var rulesWithoutExecutionOrder =
                GetRulesWithoutExecutionOrder<IRuleAsync<T>>(rules, r => !r.Parallel && !r.Configuration.ExecutionOrder.HasValue);

            var orderedAsyncRules = rulesWithExecutionOrder.Concat(rulesWithoutExecutionOrder);

            rules.Clear();

            foreach (var orderedAsyncRule in orderedAsyncRules)
            {
                rules.Add(orderedAsyncRule);
            }
        }

        private IEnumerable<IRule<T>> OrderByExecutionOrder(IEnumerable<IGeneralRule<T>> rules)
        {
            var generalRules = rules.ToList();
            var rulesWithExecutionOrder = GetRulesWithExecutionOrder<IRule<T>>(generalRules);
            var rulesWithoutExecutionOrder = GetRulesWithoutExecutionOrder<IRule<T>>(generalRules);

            return rulesWithExecutionOrder.Concat(rulesWithoutExecutionOrder);
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

        private void SynchronizeAsyncDataCollection(IRuleAsync<T> rule)
        {
            if (rule != null && !rule.Configuration.Skip)
            {
                foreach (var pair in rule.Data)
                {
                    _sharedDataAsync.TryAdd(pair.Key, pair.Value);
                }

                foreach (var key in _sharedDataAsync.Keys)
                {
                    if (!rule.Data.TryAdd(key, _sharedDataAsync[key]))
                    {
                        rule.Data.AddOrUpdate(key, v =>_sharedDataAsync[key], (k, v) => rule.Data[key]);
                    }
                }
            }
        }

        private void SynchronizeDataCollection(IRule<T> rule)
        {
            if (rule != null && !rule.Configuration.Skip)
            {
                foreach (var pair in rule.Data)
                {
                    _sharedData.TryAdd(pair.Key, pair.Value);
                }

                foreach (var key in _sharedData.Keys)
                {
                    if (!rule.Data.TryAdd(key, _sharedData[key]))
                    {
                        rule.Data.AddOrUpdate(key, v => _sharedData[key], (k, v) => rule.Data[key]);
                    }
                }
            }
        }

        private void AddToRuleResults(IRuleResult ruleResult, string ruleName)
        {
            if (ruleResult != null)
            {
                _ruleResults.Add(AssignRuleName(ruleResult, ruleName));
            }
        }

        private void AddToAsyncRuleResults(IRuleResult ruleResult, string ruleName)
        {
            if (ruleResult != null)
            {
                _asyncRuleResults.Add(AssignRuleName(ruleResult, ruleName));
            }
        }

        private IRuleResult AssignRuleName(IRuleResult ruleResult, string ruleName)
        {
            ruleResult.Name = ruleResult.Name ?? ruleName;

            return ruleResult;
        }

        private IEnumerable<TK> GetRulesWithoutExecutionOrder<TK>(IEnumerable<IGeneralRule<T>> rules,
            Func<TK, bool> condition = null) where TK : IGeneralRule<T>
        {
            condition = condition ?? (k => true);

            return rules.OfType<TK>().Where(r => !r.Configuration.ExecutionOrder.HasValue)
                .Where(condition).ToList();
        }

        private IEnumerable<TK> GetRulesWithExecutionOrder<TK>(IEnumerable<IGeneralRule<T>> rules,
            Func<TK, bool> condition = null) where TK : IGeneralRule<T>
        {
            condition = condition ?? (k => true);

            return rules.OfType<TK>()
                .Where(r => r.Configuration.ExecutionOrder.HasValue)
                .Where(condition)
                .OrderBy(r => r.Configuration.ExecutionOrder)
                .ToList();
        }

        private void Initialize(IEnumerable<IGeneralRule<T>> rules)
        {
            foreach (var rule in rules)
            {
                rule.Initialize();

                if (rule.IsNested)
                {
                    Initialize(rule.GetRules());
                }
            }
        }
    }
}