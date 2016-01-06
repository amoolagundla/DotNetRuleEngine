using System;
using System.Linq.Expressions;

namespace DotNetRuleEngine.Core.Interface
{
    public interface IConfiguration<T>
    {
        Expression<Predicate<T>> Constraint { get; set; }
        int? ExecutionOrder { get; set; }
        bool Skip { get; set; }
        bool Terminate { get; set; }
    }
}