using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DotNetRuleEngine.Core
{
    public abstract class NestedRuleAsync<T> : RuleEngine<T>,
        IRuleAsync<T> where T : class, new()
    {
        public Expression<Predicate<T>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public bool Skip { get; set; }

        public abstract Task InvokeAsync(T product);
    }
}
