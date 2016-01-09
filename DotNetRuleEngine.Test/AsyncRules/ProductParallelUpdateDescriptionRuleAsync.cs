using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductParallelUpdateDescriptionRuleAsync : RuleAsync<Product>
    {
        public override Task InitializeAsync()
        {
            Parallel = true;

            return Task.FromResult<object>(null);
        }

        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            await Task.Delay(10);
            product.Description = "Description";

            return await Task.FromResult<IRuleResult>(null);
        }
    }
}