using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    public class ProductAExecutionOrderRuleAsync : RuleAsync<Product>
    {
        public override Task InitializeAsync()
        {
            Configuration.ExecutionOrder = 2;

            return Task.FromResult<object>(null);
        }

        public override async Task<IRuleResult> InvokeAsync(Product type)
        {
            return await RuleResult.CreateAsync(new RuleResult());
        }
    }
}