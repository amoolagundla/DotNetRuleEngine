using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DotNetRuleEngine.Core
{
    public abstract class RuleAsync<T> : IRuleAsync<T> where T : class, new()
    {
        public ConcurrentDictionary<string, object> Data { get; set; } = new ConcurrentDictionary<string, object>();

        public Expression<Predicate<T>> Constraint{ get; set; }        

        public bool Terminate { get; set; }

        public bool Skip { get; set; }

        public object TryGetValue(string key)
        {
            object name;
            return Data.TryGetValue(key, out name) ? name : null;
        }

        public bool TryAdd(string key, object value)
        {
            return Data.TryAdd(key, value);
        }

        public virtual async Task BeforeInvokeAsync()
        {
            await Task.FromResult<object>(null);
        }

        public virtual async Task AfterInvokeAsync()
        {
            await Task.FromResult<object>(null);
        }

        public abstract Task<IRuleResult> InvokeAsync(T type);
        public bool Parallel { get; set; }
    }
}