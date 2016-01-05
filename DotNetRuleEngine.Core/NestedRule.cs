using System.Collections.Concurrent;

namespace DotNetRuleEngine.Core
{
    public abstract class NestedRule<T> : RuleEngine<T>, 
        IRule<T> where T : class, new()
    {
        public ConcurrentDictionary<string, object> Data { get; set; } = new ConcurrentDictionary<string, object>();

        public Configuration<T> Configuration { get; set; } = new Configuration<T>();

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

        public virtual void Initialize()
        {
        }

        public abstract IRuleResult Invoke(T type);        
    }
}
