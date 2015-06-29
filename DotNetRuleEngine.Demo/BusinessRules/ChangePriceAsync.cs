using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Demo.BusinessRules
{
    public class ChangePriceAsync : RuleAsync<Product>
    {
        public override async Task InvokeAsync(Product product)
        {
            await Task.Delay(100);
            product.Price = 12.99m;
        }
    }
}