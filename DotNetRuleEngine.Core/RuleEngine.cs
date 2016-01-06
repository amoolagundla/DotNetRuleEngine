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
        private readonly ICollection<Task<IRuleResult>> _parallelRuleResults  = new List<Task<IRuleResult>>();

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

        private async Task ExecuteAsyncRules(ICollection<IRuleAsync<T>> rules)
        {
            ExecuteParallelRules(rules);            

            var asynRules = GetAsyncRules(rules);

            foreach (var asyncRule in asynRules)
            {               
                await asyncRule.BeforeInvokeAsync();

                if (!asyncRule.Configuration.Skip && Constrained(asyncRule.Configuration.Constraint))
                {
                    var ruleResult = await asyncRule.InvokeAsync(Instance);

                    AddToAsyncRuleResults(ruleResult, asyncRule.GetType().Name);

                    if (asyncRule is INestedRuleAsync<T>)
                    {
                        await ExecuteAsyncRules(asyncRule.To<INestedRuleAsync<T>>().GetChildRules().ToList());
                    }
                }

                await asyncRule.AfterInvokeAsync();

                AddToAsyncDataCollection(asyncRule);

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

        private bool Execute(IRule<T> rule)
        {
            rule.BeforeInvoke();

            if (!rule.Configuration.Skip && Constrained(rule.Configuration.Constraint))
            {
                var ruleResult = rule.Invoke(Instance);
                AddToRuleResults(ruleResult, rule.GetType().Name);                

                if (rule is INestedRule<T>)
                {
                    var rules = OrderByExecutionOrder(rule.To<INestedRule<T>>().GetChildRules());

                    foreach (var childRule in rules)
                    {
                        Execute(childRule);
                    }
                }
            }

            rule.AfterInvoke();

            AddToDataCollection(rule);

            return rule.Configuration.Terminate;
        }

        private void ExecuteParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            var parallelRules = rules.OfType<IRuleAsync<T>>()
                .Where(rule => rule.Parallel && 
                !rule.Configuration.ExecutionOrder.HasValue).ToList();

            if (parallelRules.Any()) ExecuteParallelRule(parallelRules, _parallelRuleResults);           
        }

        private void ExecuteParallelRule(IEnumerable<IRuleAsync<T>> parallelRules, 
            ICollection<Task<IRuleResult>> parallelRuleResults)
        {
            foreach (var pRule in parallelRules)
            {
                var parallelTask = Task.Run(() =>
                {
                    return pRule.BeforeInvokeAsync()
                        .ContinueWith(a =>
                        {
                            if (!pRule.Configuration.Skip && Constrained(pRule.Configuration.Constraint))
                            {
                                return pRule.InvokeAsync(Instance).Result;
                            }
                            return Task.FromResult<IRuleResult>(null).Result;
                        })
                        .ContinueWith(a =>
                        {
                            pRule.AfterInvokeAsync();
                            return a.Result;
                        });
                });

                AddToAsyncDataCollection(pRule);

                parallelRuleResults.Add(parallelTask);

                if (pRule is INestedRuleAsync<T>)
                {
                    var parallelNestedRules = pRule.To<INestedRuleAsync<T>>().GetChildRules()
                        .Where(rule => rule.Parallel && 
                        !rule.Configuration.ExecutionOrder.HasValue &&
                        !rule.Configuration.Skip &&
                        Constrained(rule.Configuration.Constraint)).ToList();

                    ExecuteParallelRule(parallelNestedRules, parallelRuleResults);
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
            if (rule == null) return;
            
            if (rule is INestedRule<T>)
            {
                foreach (var childRule in rule.To<INestedRule<T>>().GetChildRules())
                {
                    AddToDataCollection(childRule);
                }
            }
            foreach (var pair in rule.Data)
            {
                _sharedData.TryAdd(pair.Key, pair.Value);
            }

            rule.Data = _sharedData;
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
                if (rule is INestedRule<T>)
                {
                    Initialize(rule.To<INestedRule<T>>().GetChildRules());
                }
                rule.Initialize();
            }
        }
    }
}