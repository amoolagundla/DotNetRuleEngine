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
    public sealed class RuleEngine<T> where T : class, new()
    {
        private T _instance;        
        private IDependencyResolver _dependencyResolver;
        private readonly Guid _ruleEngineId = Guid.NewGuid();
        private readonly RuleEngineConfiguration<T> _ruleEngineConfiguration = new RuleEngineConfiguration<T>(new Configuration<T>());
        private readonly ICollection<IGeneralRule<T>> _rules = new List<IGeneralRule<T>>();
        private readonly ICollection<IRuleResult> _ruleResults = new List<IRuleResult>();
        private readonly ICollection<IRuleResult> _asyncRuleResults = new List<IRuleResult>();
        private readonly ConcurrentBag<Task<IRuleResult>> _parallelRuleResults = new ConcurrentBag<Task<IRuleResult>>();
        private readonly TraceSwitch _traceSwitch = new TraceSwitch("RuleEngineRunningRuleSwitch", "RuleEngine running rules", "0");

        private const string BeforeInvoke = "BeforeInvoke";
        private const string AfterInvoke = "AFterInvoke";
        private const string Invoke = "Invoke";
        private const string Async = "Async";

        /// <summary>
        /// Rule engine ctor.
        /// </summary>
        private RuleEngine()
        {
        }

        public void SetDependencyResolver(IDependencyResolver dependencyResolver) => _dependencyResolver = dependencyResolver;

        /// <summary>
        /// Get a new instance of RuleEngine
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="dependencyResolver"></param>
        /// <returns></returns>
        public static RuleEngine<T> GetInstance(T instance, IDependencyResolver dependencyResolver = null) => 
            new RuleEngine<T> { _instance = instance, _dependencyResolver = dependencyResolver};

        /// <summary>
        /// Order of execution
        /// </summary>
        public bool InvokeNestedRulesFirst
        {
            get { return _ruleEngineConfiguration.InvokeNestedRulesFirst; }

            set { _ruleEngineConfiguration.InvokeNestedRulesFirst = value; }
        }

        /// <summary>
        /// Used to add rules to rule engine.
        /// </summary>
        /// <param name="rules">Rule(s) list.</param>
        public void AddRules(params IGeneralRule<T>[] rules)
        {
            foreach (var rule in rules)
            {
                _rules.Add(rule);
            }
        }

        /// <summary>
        /// Used to set instance.
        /// </summary>
        /// <param name="instance">_instance</param>
        public void SetInstance(T instance) => _instance = instance;

        /// <summary>
        /// Used to execute async rules.
        /// </summary>
        /// <returns></returns>
        public async Task<IRuleResult[]> ExecuteAsync()
        {
            ValidateInstance();

            if (_rules == null || !_rules.Any()) return _asyncRuleResults.ToArray();

            await InitializeAsync(_rules);

            await ExecuteAsyncRules(_rules);

            await Task.WhenAll(_parallelRuleResults);

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
        public IRuleResult[] Execute()
        {
            ValidateInstance();

            if (_rules == null || !_rules.Any()) return _ruleResults.ToArray();

            Initialize(_rules);

            Execute(OrderByExecutionOrder(_rules));

            return _ruleResults.ToArray();
        }

        private void Execute(IEnumerable<IRule<T>> rules)
        {
            foreach (var rule in rules)
            {
                InvokeNestedRules(_ruleEngineConfiguration.InvokeNestedRulesFirst, rule);

                if (CanInvoke(rule.Configuration))
                {
                    rule.Model = _instance;

                    TraceVerbose(rule, BeforeInvoke);
                    rule.BeforeInvoke();

                    TraceVerbose(rule, Invoke);
                    var ruleResult = rule.Invoke();

                    TraceVerbose(rule, AfterInvoke);
                    rule.AfterInvoke();

                    AddToRuleResults(ruleResult, rule.GetType().Name);

                    UpdateRuleEngineConfiguration(rule.Configuration);
                }

                InvokeNestedRules(!_ruleEngineConfiguration.InvokeNestedRulesFirst, rule);
            }
        }       

        private async Task ExecuteAsyncRules(ICollection<IGeneralRule<T>> rules)
        {
            await ExecuteParallelRules(rules);

            foreach (var asyncRule in OrderByAsyncRuleExecutionOrder(rules))
            {
                await InvokeNestedRulesAsync(_ruleEngineConfiguration.InvokeNestedRulesFirst, asyncRule);

                if (CanInvoke(asyncRule.Configuration))
                {
                    asyncRule.Model = _instance;

                    TraceVerbose(asyncRule, BeforeInvoke + Async);
                    await asyncRule.BeforeInvokeAsync();

                    TraceVerbose(asyncRule, Invoke + Async);
                    var ruleResult = await asyncRule.InvokeAsync();

                    TraceVerbose(asyncRule, AfterInvoke + Async);
                    await asyncRule.AfterInvokeAsync();

                    UpdateRuleEngineConfiguration(asyncRule.Configuration);

                    AddToAsyncRuleResults(ruleResult, asyncRule.GetType().Name);
                }

                await InvokeNestedRulesAsync(!_ruleEngineConfiguration.InvokeNestedRulesFirst, asyncRule);                
            }
        }

        private async Task ExecuteParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            var parallelRules = GetParallelRules(rules);

            foreach (var pRule in parallelRules)
            {
                await InvokeNestedRulesAsync(_ruleEngineConfiguration.InvokeNestedRulesFirst, pRule);

                if (CanInvoke(pRule.Configuration))
                {
                    pRule.Model = _instance;

                    var parallelTask = Task.Run(async () =>
                    {
                        TraceVerbose(pRule, BeforeInvoke + Async);
                        await pRule.BeforeInvokeAsync();

                        TraceVerbose(pRule, Invoke + Async);
                        var ruleResult = await pRule.InvokeAsync();

                        TraceVerbose(pRule, AfterInvoke + Async);
                        await pRule.AfterInvokeAsync();

                        UpdateRuleEngineConfiguration(pRule.Configuration);

                        return ruleResult;
                    });

                    _parallelRuleResults.Add(parallelTask);
                }

                await InvokeNestedRulesAsync(!_ruleEngineConfiguration.InvokeNestedRulesFirst, pRule);
            }
        }

        private async Task InvokeNestedRulesAsync(bool invokeNestedRules, IGeneralRule<T> asyncRule)
        {
            if (invokeNestedRules && asyncRule.IsNested)
            {
                await ExecuteAsyncRules(asyncRule.GetRules());
            }
        }

        private void InvokeNestedRules(bool invokeNestedRules, IGeneralRule<T> rule)
        {
            if (invokeNestedRules && rule.IsNested)
            {
                Execute(OrderByExecutionOrder(rule.GetRules()));
            }
        }

        private void ValidateInstance()
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("_instance not set");
            }
        }

        private bool Constrained(Expression<Predicate<T>> predicate)
        {
            return predicate == null || predicate.Compile().Invoke(_instance);
        }

        private void Initialize(IEnumerable<IGeneralRule<T>> rules)
        {
            foreach (var rule in rules.OfType<IRule<T>>())
            {
                rule.Model = _instance;
                rule.Configuration = new RuleEngineConfiguration<T>(rule.Configuration) { RuleEngineId = _ruleEngineId };

                rule.DependencyResolver = _dependencyResolver ?? new NullDependencyResolver();

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

                rule.DependencyResolver = _dependencyResolver ?? new NullDependencyResolver();

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

        private bool CanInvoke(IConfiguration<T> configuration) => !configuration.Skip && Constrained(configuration.Constraint) && !RuleEngineTerminated();

        private bool RuleEngineTerminated() => _ruleEngineConfiguration.Terminate != null && _ruleEngineConfiguration.Terminate.Value;

        private static IEnumerable<IRuleAsync<T>> OrderByAsyncRuleExecutionOrder(ICollection<IGeneralRule<T>> rules)
        {
            var rulesWithExecutionOrder =
                GetRulesWithExecutionOrder<IRuleAsync<T>>(rules, r => r.Configuration.ExecutionOrder.HasValue);

            var rulesWithoutExecutionOrder =
                GetRulesWithoutExecutionOrder<IRuleAsync<T>>(rules, r => !r.Parallel && !r.Configuration.ExecutionOrder.HasValue);

            return rulesWithExecutionOrder.Concat(rulesWithoutExecutionOrder).ToList();
        }

        private static IEnumerable<IRule<T>> OrderByExecutionOrder(ICollection<IGeneralRule<T>> rules)
        {
            var rulesWithExecutionOrder = GetRulesWithExecutionOrder<IRule<T>>(rules);
            var rulesWithoutExecutionOrder = GetRulesWithoutExecutionOrder<IRule<T>>(rules);

            return rulesWithExecutionOrder.Concat(rulesWithoutExecutionOrder).ToList();
        }

        private static IRuleResult AssignRuleName(IRuleResult ruleResult, string ruleName)
        {
            ruleResult.Name = ruleResult.Name ?? ruleName;

            return ruleResult;
        }

        private static ICollection<TK> GetRulesWithoutExecutionOrder<TK>(IEnumerable<IGeneralRule<T>> rules,
            Func<TK, bool> condition = null) where TK : IGeneralRule<T>
        {
            condition = condition ?? (k => true);

            return rules.OfType<TK>().Where(r => !r.Configuration.ExecutionOrder.HasValue)
                .Where(condition).ToList();
        }

        private static ICollection<TK> GetRulesWithExecutionOrder<TK>(IEnumerable<IGeneralRule<T>> rules,
            Func<TK, bool> condition = null) where TK : IGeneralRule<T>
        {
            condition = condition ?? (k => true);

            return rules.OfType<TK>()
                .Where(r => r.Configuration.ExecutionOrder.HasValue)
                .Where(condition)
                .OrderBy(r => r.Configuration.ExecutionOrder)
                .ToList();
        }

        private static IEnumerable<IRuleAsync<T>> GetParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            return rules.OfType<IRuleAsync<T>>()
                .Where(r => r.Parallel && !r.Configuration.ExecutionOrder.HasValue)
                .OrderBy(r => r.GetType().Name)
                .ToList();
        }

        private void TraceVerbose(IGeneralRule<T> rule, string message)
        {
            Trace.WriteLineIf(_traceSwitch.TraceVerbose, $"Executing {rule.GetType().Name} - {message}", "Information");
        }
    }
}