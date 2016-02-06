using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductNestedRuleAsyncB : RuleAsync<Product>
    {
        public ProductNestedRuleAsyncB()
        {
            AddRules(new ProductNestedRuleAsyncC());
        }
        public override async Task<IRuleResult> InvokeAsync()
        {
            return await Task.FromResult<IRuleResult>(null);
        }
    }
}