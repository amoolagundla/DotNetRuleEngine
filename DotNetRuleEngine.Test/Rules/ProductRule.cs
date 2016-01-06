using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductRule : Rule<Product>
    {
        public override void BeforeInvoke()
        {
            TryAdd("Key", "Value");
        }

        public override IRuleResult Invoke(Product product)
        {
            product.Description = "Product Description";

            return new RuleResult { Name = "ProductRule", Result = product.Description, Data = { { "Key", TryGetValue("Key") } } };
        }
    }
}