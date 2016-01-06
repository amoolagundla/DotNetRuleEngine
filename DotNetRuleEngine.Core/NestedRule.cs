using System.Collections.Concurrent;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core
{
    using System.Collections.Generic;

    public abstract class NestedRule<T> : INestedRule<T>
        where T : class, new()
    {
        private IList<IRule<T>> Rules { get; set; } = new List<IRule<T>>();

        public IConfiguration<T> Configuration { get; set; } = new Configuration<T>();

        public ConcurrentDictionary<string, object> Data { get; set; } = new ConcurrentDictionary<string, object>();

        public virtual void Initialize()
        {
        }

        public void BeforeInvoke()
        {
        }

        public void AfterInvoke()
        {
        }

        public abstract IRuleResult Invoke(T type);
        
        public object TryGetValue(string key)
        {
            object name;
            return Data.TryGetValue(key, out name) ? name : null;
        }

        public bool TryAdd(string key, object value)
        {
            return Data.TryAdd(key, value);
        }

        public void AddChildRules(params IRule<T>[] rules)
        {
            Rules = rules;
        }

        public IEnumerable<IRule<T>> GetChildRules()
        {
            return Rules;
        }
    }
}
