using System;
using System.Linq.Expressions;

namespace DotNetRuleEngine.Core
{
    public interface IGeneralRule<T> where T : class, new()
    {        
        Expression<Predicate<T>> Constraint { get; set; }

        bool Terminate { get; set; }

        bool Skip { get; set; }
    }
}