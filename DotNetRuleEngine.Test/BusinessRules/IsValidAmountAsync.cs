using System;
using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    public class IsValidAmountAsync : RuleAsync<Order>
    {
        public override Task BeforeInvokeAsync()
        {
            Constraint = order => order.Amount > 0.0m;
            return Task.FromResult<object>(null);
        }

        public override async Task InvokeAsync(Order order)
        {
            await Task.Delay(10);

            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
        }
    }
}