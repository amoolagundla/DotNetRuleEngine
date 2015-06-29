using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    class UpdateProductNameAsync : RuleAsync<Product>
    {
        public override async Task InvokeAsync(Product product)
        {
            product.Name = await Task.FromResult("PC");
            product.TryAdd("ProductName", product.Name);
        }
    }
}