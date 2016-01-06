using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DotNetRuleEngine.Core.Interface
{
    public interface IRuleAsync<T> : IGeneralRule<T> where T : class, new()
    {
        bool Parallel { get; set; }

        ConcurrentDictionary<string, Task<object>> Data { get; set; }

        Task BeforeInvokeAsync();

        Task AfterInvokeAsync();
        
        Task<IRuleResult> InvokeAsync(T type);

        Task<object> TryGetValueAsync(string key);

        bool TryAddAsync(string key, Task<object> value);
    }
}