using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace DotNetRuleEngine.Core
{
    public abstract class Rule<T> : IRule<T> where T : class, new()
    {
        public Expression<Predicate<T>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public bool Skip { get; set; }

        public int? ExecutionOrder { get; set; }

        public virtual void SetExecutionOrder()
        {
        }

        public object TryGetValue(string key)
        {
            return RuleDataManager.GetInstance().GetValue<T>(key);
        }

        public void TryAdd(string key, object value)
        {
            RuleDataManager.GetInstance().AddOrUpdate<T>(key, value);
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
