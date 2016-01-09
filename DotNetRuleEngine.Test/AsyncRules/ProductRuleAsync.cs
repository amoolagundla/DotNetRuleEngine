using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductRuleAsync : RuleAsync<Product>
    {
        public override Task BeforeInvokeAsync()
        {
            TryAddAsync("Description", Task.FromResult<object>("Description"));

            return null;
        }

        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            var description = TryGetValueAsync("Description").Result.To<string>();
            product.Description = $"Product {description}";            

            return await RuleResult.Create(new RuleResult
            {
                Name = "ProductRule",
                Result = product.Description,
                Data = { { "Description", description } }
            });
        }
    }
}