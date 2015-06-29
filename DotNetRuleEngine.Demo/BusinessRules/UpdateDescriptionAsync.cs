using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Demo.BusinessRules
{
    public class UpdateDescriptionAsync : RuleAsync<Product>
    {
        public override async Task InvokeAsync(Product product)
        {
            await Task.Delay(1000);
            var name = product.TryGetValue("ChangeNameAsync");
            product.Description = name +  " Desktop Computer";
        }
    }
}