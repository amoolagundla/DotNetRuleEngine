using System.Collections.Concurrent;

namespace DotNetRuleEngine.Core
{
    public interface IRule<T> : IGeneralRule<T> where T : class, new()
    {
        ConcurrentDictionary<string, object> Data { get; set; }

        void BeforeInvoke();
        
        void AfterInvoke();
        
        IRuleResult Invoke(T type);

        object TryGetValue(string key);

        bool TryAdd(string key, object value);
    }
}
