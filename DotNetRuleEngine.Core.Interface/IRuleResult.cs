using System.Collections.Generic;

namespace DotNetRuleEngine.Core.Interface
{
    public interface IRuleResult
    {
        string Name { get; set; }

        object Result { get; set; }

        Dictionary<string, object> Data { get; set; }

        IError Error { get; set; }
    }
}