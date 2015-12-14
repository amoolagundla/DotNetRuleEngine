using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductTryGetValueAsync : RuleAsync<Product>
    {
        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            product.Description = TryGetValueAsync("Description").To<string>();
            return await Task.FromResult(new RuleResult { Name = "ProductRule", Result = product.Description });
        }
    }
}