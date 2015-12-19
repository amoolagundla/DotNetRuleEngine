using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public static IRuleResult FindNestedRuleResult<T>(this IEnumerable<IRuleResult> ruleResults)
        {
            if (ruleResults == null)
            {
                return null;
            }

            foreach (var ruleResult in ruleResults)
            {
                var result = ruleResult.Result as IEnumerable<IRuleResult>;
                if (result != null)
                {
                    return FindNestedRuleResult<T>(result);
                }

                if (ruleResult.Name == typeof(T).Name)
                {
                    return ruleResult;
                }
            }
            return null;
        }

        public static IRuleResult FindNestedRuleResult(this IEnumerable<IRuleResult> ruleResults, string ruleName)
        {
            foreach (var ruleResult in ruleResults)
            {
                var result = ruleResult.Result as IEnumerable<IRuleResult>;

                if (result != null)
                {
                    return FindNestedRuleResult(result, ruleName);
                }

                if (string.Equals(ruleResult.Name, ruleName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return ruleResult;
                }
            }

            return null;
        }

        public static RuleEngineExecutor<T> ApplyRules<T>(this RuleEngineExecutor<T> ruleEngineExecutor,
            params IGeneralRule<T>[] rules) where T : class, new()
        {
            ruleEngineExecutor.AddRules(rules);
            return ruleEngineExecutor;

        }
    }
}
