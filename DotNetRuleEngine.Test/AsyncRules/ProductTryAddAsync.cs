using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductTryAddAsync : RuleAsync<Product>
    {        
        public override async Task<IRuleResult> InvokeAsync(Product type)
        {
            TryAdd("Description", "Product Description");
            return await Task.FromResult<IRuleResult>(null);
        }
    }
}