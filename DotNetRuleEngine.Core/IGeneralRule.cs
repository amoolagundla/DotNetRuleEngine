using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace DotNetRuleEngine.Core
{
    public interface IGeneralRule<T> where T : class, new()
    {
        ConcurrentDictionary<string, object> Data { get; set; }

        Expression<Predicate<T>> Constraint { get; set; }

        bool Terminate { get; set; }

        bool Skip { get; set; }

        object TryGetValue(string key);

        bool TryAdd(string key, object value);
    }
}