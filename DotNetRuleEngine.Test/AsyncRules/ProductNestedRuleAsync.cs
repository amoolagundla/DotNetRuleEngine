using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductNestedRuleAsync : NestedRuleAsync<Product>
    {
        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            Instance = product;
            AddRules(new ProductNestedRuleAsyncA(), new ProductNestedRuleAsyncB());
            var ruleResults = await ExecuteAsync();

            return await Task.FromResult(new RuleResult { Name = "ProductNestedRuleAsync", Result = ruleResults });
        }
    }
}