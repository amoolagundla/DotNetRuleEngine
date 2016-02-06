using System;

namespace DotNetRuleEngine.Core.Interface
{
    public interface IRuleEngineConfiguration<T> : IConfiguration<T>
    {
        Guid RuleEngineId { get; set; }

        bool InvokeNestedRulesFirst { get; set; }
    }
}