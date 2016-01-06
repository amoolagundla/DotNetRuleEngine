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

        public static IRuleResult[] GetErrors(this IEnumerable<IRuleResult> ruleResults)
        {
            var list = new List<IRuleResult>();
            GetErrors(ruleResults, list);

            return list.ToArray();
        }

        private static void GetErrors(IEnumerable<IRuleResult> ruleResults,
            ICollection<IRuleResult> errorResults)
        {
            foreach (var ruleResult in ruleResults)
            {
                var results = ruleResult.Result as IEnumerable<IRuleResult>;

                if (results != null)
                {
                    GetErrors(results, errorResults);
                }

                if (ruleResult.Error != null)
                {
                    errorResults.Add(ruleResult);
                }
            }
        }
    }
}
