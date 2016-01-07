using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductSkipAsync : RuleAsync<Product>
    {
        public override void Initialize()
        {
            Configuration.Skip = true;
        }

        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            product.Description = "Product Description";
            return await Task.FromResult(new RuleResult { Name = "ProductRule", Result = product.Description });
        }
    }
}