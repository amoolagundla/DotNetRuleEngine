using System.Collections.Generic;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core
{
    public class RuleResult : IRuleResult
    {
        public RuleResult()
        {
            Data = new Dictionary<string, object>();
        }

        public string Name { get; set; }

        public object Result { get; set; }

        public Dictionary<string, object> Data { get; set; }

        public IError Error { get; set; }
    }
}
