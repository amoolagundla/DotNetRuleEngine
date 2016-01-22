using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DotNetRuleEngine.Core
{
    public abstract class RuleAsync<T> : IRuleAsync<T> where T : class, new()
    {
        public Expression<Predicate<T>> Constraint{ get; set; }        

        public bool Terminate { get; set; }

        public bool Skip { get; set; }

        public int? ExecutionOrder { get; set; }

        public virtual void SetExecutionOrder()
        {
        }

        public async Task<object> TryGetValueAsync(string key)
        {
            return await RuleDataManager.GetInstance().GetValueAsync<T>(key);
        }

        public async Task TryAddAsync(string key, Task<object> value)
        {
            await RuleDataManager.GetInstance().AddOrUpdateAsync<T>(key, value);
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