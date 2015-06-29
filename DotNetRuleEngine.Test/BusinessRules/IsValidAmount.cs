using System;
using System.Diagnostics;
using System.Linq.Expressions;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    public class IsValidAmount : Rule<Order>
    {
        private Stopwatch _stopwatch;
        public override void Invoke(Order bRule)
        {
            if (bRule.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
        }

        public override void BeforeInvoke()
        {
            _stopwatch = Stopwatch.StartNew();
            Constraint = order => order.Amount > 0.0m;
        }

        public override void AfterInvoke()
        {
            _stopwatch.Stop();
            var elapsed = _stopwatch.ElapsedMilliseconds;
        }
    }
}
