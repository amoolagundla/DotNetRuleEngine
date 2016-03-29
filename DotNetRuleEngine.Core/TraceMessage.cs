using System.Diagnostics;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core
{
    internal static class TraceMessage
    {
        public const string BeforeInvoke = "BeforeInvoke";
        public const string AfterInvoke = "AfterInvoke";
        public const string Invoke = "Invoke";
        public const string Async = "Async";
        private static readonly TraceSwitch TraceSwitch = 
            new TraceSwitch("RuleEngineRunningRuleSwitch", "RuleEngine running rules", "0");

        public static void Verbose<T>(IGeneralRule<T> rule, string message) where T : class, new()
        {
            Trace.WriteLineIf(TraceSwitch.TraceVerbose, $"Executing {rule.GetType().Name} - {message}", "Information");
        }
    }
}
