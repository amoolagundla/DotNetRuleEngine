using System;
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
        private readonly Guid _ruleEngineId = Guid.NewGuid();
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

            await InitializeAsync(Rules);

            ExecuteParallelRules(Rules);

            if (_parallelRuleResults.Any())
            {
                await Task.WhenAll(_parallelRuleResults);
            }

            await ExecuteAsyncRules(OrderByAsyncRuleExecutionOrder(Rules));

            _parallelRuleResults.ToList().ForEach(rule =>
            {
                AddToAsyncRuleResults(rule.Result, rule.GetType().Name);
            });

            return _asyncRuleResults.ToArray();
        }

        private async Task ExecuteAsyncRules(ICollection<IRuleAsync<T>> rules)
        {
            foreach (var asyncRule in rules)
            {
                if (!asyncRule.Configuration.Skip && Constrained(asyncRule.Configuration.Constraint))
                {
                    if (asyncRule.IsNested)
                    {
                        await ExecuteAsyncRules(OrderByAsyncRuleExecutionOrder(asyncRule.GetRules()));
                    }

                    await asyncRule.BeforeInvokeAsync();

                    var ruleResult = await asyncRule.InvokeAsync(Instance);

                    AddToAsyncRuleResults(ruleResult, asyncRule.GetType().Name);

                    await asyncRule.AfterInvokeAsync();
                }

                if (asyncRule.Configuration.Terminate)
                {
                    break;
                }
            }
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

        private bool Execute(IRule<T> rule)
        {
            if (!rule.Configuration.Skip && Constrained(rule.Configuration.Constraint))
            {
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

            return rule.Configuration.Terminate;
        }

        private void ExecuteParallelRules(ICollection<IGeneralRule<T>> rules)
        {
            var parallelRules = GetParallelRules(rules);

            foreach (var pRule in parallelRules)
            {
                if (!pRule.Configuration.Skip && Constrained(pRule.Configuration.Constraint))
                {
                    var parallelTask = Task.Run(async () =>
                    {
                        await pRule.BeforeInvokeAsync();

                        var ruleResult = await pRule.InvokeAsync(Instance);

                        await pRule.AfterInvokeAsync();

                        return ruleResult;
                    });

                    _parallelRuleResults.Add(parallelTask);
                }
            }
        }

        private ICollection<IRuleAsync<T>> OrderByAsyncRuleExecutionOrder(ICollection<IGeneralRule<T>> rules)
        {
            var rulesWithExecutionOrder =
                GetRulesWithExecutionOrder<IRuleAsync<T>>(rules, r => r.Configuration.ExecutionOrder.HasValue);

            var rulesWithoutExecutionOrder =
                GetRulesWithoutExecutionOrder<IRuleAsync<T>>(rules, r => !r.Parallel && !r.Configuration.ExecutionOrder.HasValue);

            return rulesWithExecutionOrder.Concat(rulesWithoutExecutionOrder).ToList();
        }

        private ICollection<IRule<T>> OrderByExecutionOrder(ICollection<IGeneralRule<T>> rules)
        {
            var rulesWithExecutionOrder = GetRulesWithExecutionOrder<IRule<T>>(rules);
            var rulesWithoutExecutionOrder = GetRulesWithoutExecutionOrder<IRule<T>>(rules);

            return rulesWithExecutionOrder.Concat(rulesWithoutExecutionOrder).ToList();
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

        private IRuleResult AssignRuleName(IRuleResult ruleResult, string ruleName)
        {
            ruleResult.Name = ruleResult.Name ?? ruleName;

            return ruleResult;
        }

        private ICollection<TK> GetRulesWithoutExecutionOrder<TK>(ICollection<IGeneralRule<T>> rules,
            Func<TK, bool> condition = null) where TK : IGeneralRule<T>
        {
            condition = condition ?? (k => true);

            return rules.OfType<TK>().Where(r => !r.Configuration.ExecutionOrder.HasValue)
                .Where(condition).ToList();
        }

        private ICollection<TK> GetRulesWithExecutionOrder<TK>(ICollection<IGeneralRule<T>> rules,
            Func<TK, bool> condition = null) where TK : IGeneralRule<T>
        {
            condition = condition ?? (k => true);

            return rules.OfType<TK>()
                .Where(r => r.Configuration.ExecutionOrder.HasValue)
                .Where(condition)
                .OrderBy(r => r.Configuration.ExecutionOrder)
                .ToList();
        }

        private ICollection<IRuleAsync<T>> GetParallelRules(ICollection<IGeneralRule<T>> rules)
        {
            return GetParallelRules(rules, new List<IRuleAsync<T>>());
        }

        private ICollection<IRuleAsync<T>> GetParallelRules(ICollection<IGeneralRule<T>> rules,
            ICollection<IRuleAsync<T>> parallelRules)
        {
            foreach (var rule in rules.OfType<IRuleAsync<T>>())
            {
                if (rule.IsNested)
                {
                    GetParallelRules(rule.GetRules(), parallelRules);
                }
                if (rule.Parallel && !rule.Configuration.ExecutionOrder.HasValue)
                {
                    parallelRules.Add(rule);
                }
            }

            return parallelRules.ToList();
        }

        private void Initialize(ICollection<IGeneralRule<T>> rules)
        {
            foreach (var rule in rules.OfType<IRule<T>>())
            {
                rule.Configuration = new RuleEngineConfiguration<T>(rule.Configuration) { RuleEngineId = _ruleEngineId };

                rule.Initialize();

                if (rule.IsNested)
                {
                    Initialize(rule.GetRules());
                }
            }
        }

        private async Task InitializeAsync(ICollection<IGeneralRule<T>> rules)
        {
            foreach (var rule in rules.OfType<IRuleAsync<T>>())
            {
                rule.Configuration = new RuleEngineConfiguration<T>(rule.Configuration) { RuleEngineId = _ruleEngineId };

                await rule.InitializeAsync();

                if (rule.IsNested)
                {
                    await InitializeAsync(rule.GetRules());
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
    }
}