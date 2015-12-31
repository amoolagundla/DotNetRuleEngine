using System;

namespace DotNetRuleEngine.Core
{
    public class Error : IError
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
