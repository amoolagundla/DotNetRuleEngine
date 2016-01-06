using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core
{
    public abstract class NestedRuleAsync<T> : INestedRuleAsync<T> where T : class, new()
    {
        private IList<IRuleAsync<T>> Rules { get; set; } = new List<IRuleAsync<T>>();

        public ConcurrentDictionary<string, Task<object>> Data { get; set; } = new ConcurrentDictionary<string, Task<object>>();

        public IConfiguration<T> Configuration { get; set; } = new Configuration<T>();        

        public async Task<object> TryGetValueAsync(string key)
        {
            Task<object> value;
            return Data.TryGetValue(key, out value) ? await value : null;
        }

        public bool TryAddAsync(string key, Task<object> value)
        {
            return Data.TryAdd(key, value);
        }

        public virtual void Initialize()
        {
        }

        public virtual async Task BeforeInvokeAsync()
        {
            await Task.FromResult<object>(null);
        }

        public virtual async Task AfterInvokeAsync()
        {
            await Task.FromResult<object>(null);
        }

        public void AddChildRules(params IRuleAsync<T>[] rules)
        {
            Rules = rules;
        }

        public IEnumerable<IRuleAsync<T>> GetChildRules()
        {
            return Rules;
        }

        public abstract Task<IRuleResult> InvokeAsync(T type);

        public bool Parallel { get; set; }
    }
}
