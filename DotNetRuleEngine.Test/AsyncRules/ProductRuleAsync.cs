using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductRuleAsync : RuleAsync<Product>
    {
        public override Task BeforeInvokeAsync()
        {
            TryAdd("Key", "Value");

            return Task.FromResult<object>(null);
        }

        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            product.Description = "Product Description";

            return await Task.FromResult(new RuleResult { Name = "ProductRule", Result = product.Description, Data = { { "Key", TryGetValue("Key") } } });
        }
    }
}