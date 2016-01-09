using System.Threading.Tasks;

namespace DotNetRuleEngine.Core.Interface
{
    public interface IRuleAsync<T> : IGeneralRule<T> where T : class, new()
    {
        bool Parallel { get; set; }

        Task InitializeAsync();

        Task BeforeInvokeAsync();

        Task AfterInvokeAsync();
        
        Task<IRuleResult> InvokeAsync(T type);

        Task<object> TryGetValueAsync(string key);

        void TryAddAsync(string key, Task<object> value);
    }
}