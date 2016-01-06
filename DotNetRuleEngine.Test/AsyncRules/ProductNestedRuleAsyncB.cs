using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductNestedRuleAsyncB : NestedRuleAsync<Product>
    {
        public ProductNestedRuleAsyncB()
        {
            AddChildRules(new ProductNestedRuleAsyncC());
        }
        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            return await Task.FromResult<IRuleResult>(null);
        }
    }
}