using System;
using System.Linq.Expressions;

namespace DotNetRuleEngine.Core
{
    public class Configuration<T>
    {
        public Expression<Predicate<T>> Constraint { get; set; }

        public bool Skip { get; set; }

        public bool Terminate { get; set; }

        public int? ExecutionOrder { get; set; }
    }
}
