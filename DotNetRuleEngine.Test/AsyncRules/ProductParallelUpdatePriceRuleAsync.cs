using System.Diagnostics;
using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductParallelUpdatePriceRuleAsync : RuleAsync<Product>
    {
        public ProductParallelUpdatePriceRuleAsync()
        {
            Parallel = true;
        }
        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            await Task.Delay(5);
            product.Price = 0.0m;
            Debug.WriteLine("ProductParallelUpdatePriceRuleAsync");
            return await Task.FromResult<IRuleResult>(null);
        }
    }
}