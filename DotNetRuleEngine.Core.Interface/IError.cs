using System;

namespace DotNetRuleEngine.Core.Interface
{
    public interface IError
    {
        string Message { get; set; }

        Exception Exception { get; set; }
    }
}