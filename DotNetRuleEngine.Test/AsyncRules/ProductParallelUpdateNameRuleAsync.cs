using System.Diagnostics;
using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductParallelUpdateNameRuleAsync : RuleAsync<Product>
    {
        public ProductParallelUpdateNameRuleAsync()
        {
            Parallel = true;
        }

        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            await Task.Delay(15);
            product.Name = "Product";
            Debug.WriteLine("ProductParallelUpdateNameRuleAsync");
            return await Task.FromResult<IRuleResult>(null);
        }
    }
}