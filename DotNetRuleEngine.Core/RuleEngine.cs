using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly RuleEngineConfiguration<T> _ruleEngineConfiguration = new RuleEngineConfiguration<T>(new Configuration<T> ());
        private readonly ICollection<IRuleResult> _ruleResults = new List<IRuleResult>();
        private readonly ICollection<IRuleResult> _asyncRuleResults = new List<IRuleResult>();
        private readonly ConcurrentBag<Task<IRuleResult>> _parallelRuleResults = new ConcurrentBag<Task<IRuleResult>>();

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

            await ExecuteAsyncRules(Rules);            

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

            Execute(OrderByExecutionOrder(Rules));

            return _ruleResults.ToArray();
        }

        private void Execute(ICollection<IRule<T>> rules)
        {
            foreach (var rule in rules)
            {
                if (CanExecute(rule.Configuration))
                {
                    if (rule.IsNested)
                    {
                        Execute(OrderByExecutionOrder(rule.GetRules()));
                    }

                    rule.BeforeInvoke();

                    var ruleResult = rule.Invoke(Instance);

                    rule.AfterInvoke();

                    AddToRuleResults(ruleResult, rule.GetType().Name);

                    UpdateRuleEngineConfiguration(rule.Configuration);
                }
            }
        }

        private async Task ExecuteAsyncRules(ICollection<IGeneralRule<T>> rules)
        {
            await ExecuteParallelRules(rules);

            foreach (var asyncRule in OrderByAsyncRuleExecutionOrder(rules))
            {
                if (CanExecute(asyncRule.Configuration))
                {
                    if (asyncRule.IsNested)
                    {
                        await ExecuteAsyncRules(asyncRule.GetRules());
                    }

                    await asyncRule.BeforeInvokeAsync();

                    var ruleResult = await asyncRule.InvokeAsync(Instance);

                    await asyncRule.AfterInvokeAsync();

                    UpdateRuleEngineConfiguration(asyncRule.Configuration);

                    AddToAsyncRuleResults(ruleResult, asyncRule.GetType().Name);
                }
            }
        }

        private async Task ExecuteParallelRules(ICollection<IGeneralRule<T>> rules)
        {
            var parallelRules = GetParallelRules(rules);

            foreach (var pRule in parallelRules)
            {
                if (CanExecute(pRule.Configuration))
                {
                    if (pRule.IsNested)
                    {
                        await ExecuteAsyncRules(pRule.GetRules());
                    }

                    var parallelTask = Task.Run(async () =>
                    {
                        await pRule.BeforeInvokeAsync();

                        var ruleResult = await pRule.InvokeAsync(Instance);

                        await pRule.AfterInvokeAsync();

                        UpdateRuleEngineConfiguration(pRule.Configuration);

                        return ruleResult;
                    });

                    _parallelRuleResults.Add(parallelTask);
                }
            }

            await Task.WhenAll(_parallelRuleResults);
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
            return rules.OfType<IRuleAsync<T>>()
                .Where(r => r.Parallel && !r.Configuration.ExecutionOrder.HasValue)
                .OrderBy(r => r.GetType().Name)
                .ToList();
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

        private void UpdateRuleEngineConfiguration(IConfiguration<T> ruleConfiguration)
        {
            if (_ruleEngineConfiguration.Terminate == null && ruleConfiguration.Terminate == true)
            {
                _ruleEngineConfiguration.Terminate = true;
            }
        }

        private bool CanExecute(IConfiguration<T> configuration)
        {
            return !configuration.Skip && Constrained(configuration.Constraint) && !RuleEngineTerminated();
        }

        private bool RuleEngineTerminated()
        {
            return _ruleEngineConfiguration.Terminate != null && _ruleEngineConfiguration.Terminate.Value;
        }
    }
}