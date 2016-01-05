using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DotNetRuleEngine.Core
{
    public abstract class RuleAsync<T> : IRuleAsync<T> where T : class, new()
    {
        public ConcurrentDictionary<string, Task<object>> Data { get; set; } = new ConcurrentDictionary<string, Task<object>>();

        public Configuration<T> Configuration { get; set; } = new Configuration<T>();

        public async Task<object> TryGetValueAsync(string key)
        {
            Task<object> value;
            return Data.TryGetValue(key, out value) ? await value : null;
        }

        public bool TryAddAsync(string key, Task<object> value)
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

        public virtual void Initialize()
        {
        }

        public abstract Task<IRuleResult> InvokeAsync(T type);
        

        public bool Parallel { get; set; }        
    }
}