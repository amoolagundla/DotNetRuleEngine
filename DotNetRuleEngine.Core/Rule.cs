using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DotNetRuleEngine.Core
{
    public abstract class Rule<T> : IRule<T> where T : class, new()
    {
        public Expression<Predicate<T>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public bool Skip { get; set; }

        public virtual void BeforeInvoke()
        {
        }

        public virtual void AfterInvoke()
        {
        }

        public abstract void Invoke(T type);
    }
}
