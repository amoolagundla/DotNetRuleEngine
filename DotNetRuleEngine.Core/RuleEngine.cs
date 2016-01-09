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

            var asynRules = GetAsyncRules(Rules);

            await InitializeAsync(asynRules);

            ExecuteParallelRules(asynRules);

            if (_parallelRuleResults.Any())
            {
                await Task.WhenAll(_parallelRuleResults);
            }

            await ExecuteAsyncRules(OrderByAsyncRuleExecutionOrder(asynRules));

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

        private async Task ExecuteAsyncRules(IReadOnlyCollection<IRuleAsync<T>> rules)
        {
            foreach (var asyncRule in rules)
            {
                if (!asyncRule.Configuration.Skip && Constrained(asyncRule.Configuration.Constraint))
                {
                    if (asyncRule.IsNested)
                    {
                        await ExecuteAsyncRules(OrderByAsyncRuleExecutionOrder(GetAsyncRules(asyncRule.GetRules()).ToList()));
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

        private void ExecuteParallelRules(IEnumerable<IGeneralRule<T>> rules)
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

        private IReadOnlyCollection<IRuleAsync<T>> OrderByAsyncRuleExecutionOrder(IReadOnlyCollection<IRuleAsync<T>> rules)
        {
            var rulesWithExecutionOrder =
                GetRulesWithExecutionOrder<IRuleAsync<T>>(rules, r => r.Configuration.ExecutionOrder.HasValue);

            var rulesWithoutExecutionOrder =
                GetRulesWithoutExecutionOrder<IRuleAsync<T>>(rules, r => !r.Parallel && !r.Configuration.ExecutionOrder.HasValue);

            return rulesWithExecutionOrder.Concat(rulesWithoutExecutionOrder).ToList();
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

        private IReadOnlyCollection<TK> GetRulesWithoutExecutionOrder<TK>(IEnumerable<IGeneralRule<T>> rules,
            Func<TK, bool> condition = null) where TK : IGeneralRule<T>
        {
            condition = condition ?? (k => true);

            return rules.OfType<TK>().Where(r => !r.Configuration.ExecutionOrder.HasValue)
                .Where(condition).ToList();
        }

        private IReadOnlyCollection<TK> GetRulesWithExecutionOrder<TK>(IEnumerable<IGeneralRule<T>> rules,
            Func<TK, bool> condition = null) where TK : IGeneralRule<T>
        {
            condition = condition ?? (k => true);

            return rules.OfType<TK>()
                .Where(r => r.Configuration.ExecutionOrder.HasValue)
                .Where(condition)
                .OrderBy(r => r.Configuration.ExecutionOrder)
                .ToList();
        }

        private IReadOnlyCollection<IRuleAsync<T>> GetAsyncRules(IEnumerable<IGeneralRule<T>> rules)
        {
            var asyncRules = rules.OfType<IRuleAsync<T>>()
                .Where(rule => !rule.Parallel)
                .ToList();

            OrderByAsyncRuleExecutionOrder(asyncRules);

            return asyncRules;
        }

        private IReadOnlyCollection<IRuleAsync<T>> GetParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            return GetParallelRules(rules, new List<IRuleAsync<T>>());
        }

        private IReadOnlyCollection<IRuleAsync<T>> GetParallelRules(IEnumerable<IGeneralRule<T>> rules,
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

        private void Initialize(IEnumerable<IGeneralRule<T>> rules)
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

        private async Task InitializeAsync(IEnumerable<IGeneralRule<T>> rules)
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
    }
}