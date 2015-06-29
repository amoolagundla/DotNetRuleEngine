using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Demo.BusinessRules
{
    public class ChangeNameAsync : RuleAsync<Product>
    {
        public override async Task InvokeAsync(Product product)
        {
            await Task.Delay(100);
            product.Name = "PC";

            product.TryAdd("ChangeNameAsync", "PC");
        }
    }
}