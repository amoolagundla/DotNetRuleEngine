using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace DotNetRuleEngine.Core
{
    public abstract class Rule<T> : IRule<T> where T : class, new()
    {
        public ConcurrentDictionary<string, object> Data { get; set; } = new ConcurrentDictionary<string, object>();

        public Expression<Predicate<T>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public bool Skip { get; set; }

        public int? ExecutionOrder { get; set; }

        public virtual void SetExecutionOrder()
        {
        }

        public object TryGetValue(string key)
        {
            object name;
            return Data.TryGetValue(key, out name) ? name : null;
        }

        public bool TryAdd(string key, object value)
        {
            return Data.TryAdd(key, value);
        }

        public virtual void BeforeInvoke()
        {
        }

        public virtual void AfterInvoke()
        {
        }

        public abstract IRuleResult Invoke(T type);
    }
}
