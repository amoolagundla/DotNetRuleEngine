using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace DotNetRuleEngine.Core
{
    public abstract class NestedRule<T> : RuleEngine<T>, 
        IRule<T> where T : class, new()
    {
        public ConcurrentDictionary<string, object> Data { get; set; }

        public Expression<Predicate<T>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public bool Skip { get; set; }

        public abstract void Invoke(T type);
    }
}
