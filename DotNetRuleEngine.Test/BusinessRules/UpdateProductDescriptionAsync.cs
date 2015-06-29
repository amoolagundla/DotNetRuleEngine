using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    class UpdateProductDescriptionAsync : RuleAsync<Product>
    {
        public override async Task InvokeAsync(Product product)
        {
            var name = product.TryGetValue("ProductName");
            product.Description = await Task.FromResult("Personal Computer");
        }
    }
}