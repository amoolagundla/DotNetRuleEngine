using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductNestedRuleAsyncB : NestedRuleAsync<Product>
    {
        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            Instance = product;
            AddRules(new ProductNestedRuleAsyncC());
            var ruleResults = await ExecuteAsync();

            return await Task.FromResult(new RuleResult { Name = "ProductNestedRuleAsyncB", Result = ruleResults });
        }
    }
}