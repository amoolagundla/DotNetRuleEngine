using DotNetRuleEngine.Core;
using Product = DotNetRuleEngine.Test.Models.Product;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductTryAdd : Rule<Product>
    {
        public override IRuleResult Invoke(Product product)
        {
            TryAdd("Description", "Product Description");
            return null;
        }
    }
}