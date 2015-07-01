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

        protected RuleEngine()
        {
            Instance = this as T;
        }

        public T Instance { get; set; }

        protected IList<IGeneralRule<T>> Rules { get; private set; }

        public object TryGetValue(string key)
        {
            object name;

            return _data.TryGetValue(key, out name) ? name : null;
        }

        public bool TryAdd(string key, object value)
        {
            return _data.TryAdd(key, value);
        }

        public virtual void AddRules(params IGeneralRule<T>[] rules)
        {
            Rules = rules.ToList();
        }

        public virtual async Task ExecuteAsync()
        {
            ValidateInstance();

            if (Rules != null && Rules.Any())
            {
                foreach (var asyncRule in Rules.OfType<IRuleAsync<T>>())
                {
                    if (Constrained(asyncRule.Constraint))
                    {
                        AddRuleAsyncDataValues(asyncRule);
                        await ExecuteBeforeInvokeAsyncEvent(asyncRule);

                        if (!asyncRule.Skip)
                        {
                            await asyncRule.InvokeAsync(Instance);                            
                        }

                        await ExecuteAfterInvokeAsyncEvent(asyncRule);
                    }
                    if (asyncRule.Terminate)
                    {
                        break;
                    }
                }
            }
        }

        public virtual void Execute()
        {
            ValidateInstance();

            if (Rules != null && Rules.Any())
            {
                foreach (var rule in Rules.OfType<IRule<T>>())
                {
                    if (Constrained(rule.Constraint))
                    {
                        AddRuleDataValues(rule);
                        ExecuteBeforeInvokeEvent(rule);

                        if (!rule.Skip)
                        {
                            rule.Invoke(Instance);
                        }

                        ExecuteAfterInvokeEvent(rule);
                    }
                    if (rule.Terminate)
                    {
                        break;
                    }
                }
            }
        }

        private void AddRuleDataValues(IGeneralRule<T> rule)
        {
            var tmp = rule as Rule<T>;
            AddToDataCollection(tmp);
        }

        private void AddRuleAsyncDataValues(IGeneralRule<T> rule)
        {
            var tmp = rule as RuleAsync<T>;
            AddToDataCollection(tmp);
        }

        private void ExecuteAfterInvokeEvent(IGeneralRule<T> rule)
        {
            var tmp = rule as Rule<T>;
            if (tmp != null)
            {
                tmp.AfterInvoke();
            }
        }

        private void ExecuteBeforeInvokeEvent(IGeneralRule<T> rule)
        {
            var tmp = rule as Rule<T>;
            if (tmp != null)
            {
                tmp.BeforeInvoke();
            }
        }

        private async Task ExecuteAfterInvokeAsyncEvent(IGeneralRule<T> rule)
        {
            var tmp = rule as RuleAsync<T>;
            if (tmp != null)
            {
                await tmp.AfterInvokeAsync();
            }
        }

        private async Task ExecuteBeforeInvokeAsyncEvent(IGeneralRule<T> rule)
        {
            var tmp = rule as RuleAsync<T>;
            if (tmp != null)
            {
                await tmp.BeforeInvokeAsync();
            }
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