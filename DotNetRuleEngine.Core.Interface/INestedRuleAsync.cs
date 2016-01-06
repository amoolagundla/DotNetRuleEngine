using System.Collections.Generic;

namespace DotNetRuleEngine.Core.Interface
{
    public interface INestedRuleAsync<T> : IRuleAsync<T> where T : class, new()
    {
        IEnumerable<IRuleAsync<T>> GetChildRules();

        void AddChildRules(params IRuleAsync<T>[] rules);
    }
}