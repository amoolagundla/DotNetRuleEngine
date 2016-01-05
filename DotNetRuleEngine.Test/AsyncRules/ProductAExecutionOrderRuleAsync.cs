using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    public class ProductAExecutionOrderRuleAsync : RuleAsync<Product>
    {
        public override void Initialize()
        {
            Configuration.ExecutionOrder = 2;
        }

        public override async Task<IRuleResult> InvokeAsync(Product type)
        {
            return await Task.FromResult(new RuleResult());
        }
    }
}