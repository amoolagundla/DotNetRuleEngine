using System.Collections.Generic;

namespace DotNetRuleEngine.Core.Interface
{
    public interface INestedRule<T> : IRule<T> where T : class, new()
    {
        IEnumerable<IRule<T>> GetChildRules();

        void AddChildRules(params IRule<T>[] rules);
    }
}