using System;
using System.Collections.Concurrent;

namespace DotNetRuleEngine.Core
{
    public class NestedRule2<T> : IRule<T>
        where T : class, new()
    {
        public Configuration<T> Configuration { get; set; }

        public ConcurrentDictionary<string, object> Data { get; set; }

        public void BeforeInvoke()
        {
            throw new NotImplementedException();
        }

        public void AfterInvoke()
        {
            throw new NotImplementedException();
        }

        public IRuleResult Invoke(T type)
        {
            throw new NotImplementedException();
        }

        public object TryGetValue(string key)
        {
            throw new NotImplementedException();
        }

        public virtual void Initialize()
        {
        }

        public bool TryAdd(string key, object value)
        {
            throw new NotImplementedException();
        }
    }
}
