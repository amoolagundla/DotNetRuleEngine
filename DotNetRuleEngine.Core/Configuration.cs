using System;
using System.Linq.Expressions;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core
{
    public class Configuration<T> : IConfiguration<T>
    {
        public Expression<Predicate<T>> Constraint { get; set; }

        public bool Skip { get; set; }

        public bool? Terminate { get; set; }

        public int? ExecutionOrder { get; set; }
    }
}
