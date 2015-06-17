using System;
using System.Linq.Expressions;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    public class IsValidAmount : IRule<Order>
    {
        public Expression<Predicate<Order>> Constraint { get; set; }

        public bool Terminate { get; set; }
        

        public void Invoke(Order bRule)
        {
            if (bRule.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
