using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DotNetRuleEngine.Core
{
    public interface IRuleAsync<T> : IGeneralRule<T> where T : class, new()
    {
        bool Parallel { get; set; }

        Task BeforeInvokeAsync();

        Task AfterInvokeAsync();
        
        Task<IRuleResult> InvokeAsync(T product);

        Task<object> TryGetValueAsync(string key);

        Task TryAddAsync(string key, Task<object> value);
    }
}