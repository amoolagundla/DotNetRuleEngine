using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductParallelUpdateDescriptionRuleAsync : RuleAsync<Product>
    {
        public ProductParallelUpdateDescriptionRuleAsync()
        {
            Parallel = true;
        }
        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            await Task.Delay(10);
            product.Description = "Description";
            Debug.WriteLine("ProductParallelUpdateDescriptionRuleAsync");

            return await Task.FromResult<IRuleResult>(null);
        }
    }
}