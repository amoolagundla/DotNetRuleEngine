using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core
{
    public static class Extensions
    {
        public static T To<T>(this object @object)
        {
            return @object != null ? (T)@object : default(T);
        }

        public static T To<T>(this Task<object> @object)
        {
            return @object != null ? (T)@object.Result : default(T);
        }

        public static IRuleResult FindRuleResult<T>(this IEnumerable<IRuleResult> ruleResults)
        {
            return ruleResults.FirstOrDefault(r => string.Equals(r.Name, typeof(T).Name, StringComparison.InvariantCultureIgnoreCase));
        }

        public static IRuleResult FindRuleResult(this IEnumerable<IRuleResult> ruleResults, string ruleName)
        {
            return ruleResults.FirstOrDefault(r => string.Equals(r.Name, ruleName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static RuleEngineExecutor<T> ApplyRules<T>(this RuleEngineExecutor<T> ruleEngineExecutor,
            params IGeneralRule<T>[] rules) where T : class, new()
        {
            ruleEngineExecutor.AddRules(rules);

            return ruleEngineExecutor;
        }

        public static IEnumerable<IRuleResult> GetErrors(this IEnumerable<IRuleResult> ruleResults)
        {
            return ruleResults.Where(r => r.Error != null);
        }
    }
}
