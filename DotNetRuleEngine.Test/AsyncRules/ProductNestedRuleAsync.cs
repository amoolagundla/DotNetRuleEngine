using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductNestedRuleAsync : RuleAsync<Product>
    {
        public ProductNestedRuleAsync()
        {
            AddRules(new ProductNestedRuleAsyncA(), new ProductNestedRuleAsyncB());
        }
        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            return await Task.FromResult<IRuleResult>(null);
        }
    }
}