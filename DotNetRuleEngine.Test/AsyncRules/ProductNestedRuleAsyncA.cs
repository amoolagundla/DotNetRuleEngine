using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductNestedRuleAsyncA : RuleAsync<Product>
    {
        public override async Task<IRuleResult> InvokeAsync()
        {
            Model.Description = "Product Description";

            return await Task.FromResult(new RuleResult { Name = "ProductNestedRuleAsyncA", Result = Model.Description });
        }
    }
}