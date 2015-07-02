using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Demo.BusinessRules
{
    public class ChangeNameAsync : RuleAsync<Product>
    {
        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            await Task.Delay(100);
            product.Name = "PC";

            TryAdd("ChangeNameAsync", "PC");

            return null;
        }
    }
}