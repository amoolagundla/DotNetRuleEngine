using DotNetRuleEngine.Core;
using Product = DotNetRuleEngine.Test.Models.Product;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductTryAdd : Rule<Product>
    {
        public ProductTryAdd()
        {
            TryAdd("Description1", "Product Description1");
        }

        public override void BeforeInvoke()
        {
            TryAdd("Description2", "Product Description2");
        }

        public override IRuleResult Invoke(Product product)
        {
            TryAdd("Description3", "Product Description3");
            return null;
        }

        public override void AfterInvoke()
        {
            TryAdd("Description4", "Product Description4");
        }
    }
}