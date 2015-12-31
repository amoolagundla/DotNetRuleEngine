using System;

namespace DotNetRuleEngine.Core
{
    public interface IError
    {
        string Message { get; set; }

        Exception Exception { get; set; }
    }
}