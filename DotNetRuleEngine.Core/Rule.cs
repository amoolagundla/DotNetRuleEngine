using System.Collections.Concurrent;
using System.Linq;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core
{
    using System.Collections.Generic;

    public abstract class Rule<T> : IRule<T>
        where T : class, new()
    {
        private IList<IGeneralRule<T>> Rules { get; set; } = new List<IGeneralRule<T>>();

        public bool IsNested => Rules.Any();

        public IConfiguration<T> Configuration { get; set; } = new Configuration<T>();

        public ConcurrentDictionary<string, object> Data { get; set; } = new ConcurrentDictionary<string, object>();

        public virtual void Initialize()
        {
        }
        public virtual void BeforeInvoke()
        {
        }

        public virtual void AfterInvoke()
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

        public void AddRules(params IGeneralRule<T>[] rules)
        {
            Rules = rules;
        }

        public IEnumerable<IGeneralRule<T>> GetRules()
        {
            return Rules;
        }
    }
}
