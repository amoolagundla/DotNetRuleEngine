using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductConstraintAsyncB : RuleAsync<Product>
    {
        public override Task BeforeInvokeAsync()
        {
            Configuration.Constraint = product => Model.Description == "";
            return Task.FromResult<object>(null);
        }

        public override async Task<IRuleResult> InvokeAsync()
        {
            Model.Description = "Product Description";
            return await Task.FromResult(new RuleResult { Name = "ProductRule", Result = Model.Description });
        }
    }
}