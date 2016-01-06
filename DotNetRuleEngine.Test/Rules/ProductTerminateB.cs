using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using Product = DotNetRuleEngine.Test.Models.Product;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductTerminateB : Rule<Product>
    {
        public override IRuleResult Invoke(Product product)
        {
            product.Description = "Product Description";
            return new RuleResult { Name = "ProductRule", Result = product.Description };
        }
    }
}