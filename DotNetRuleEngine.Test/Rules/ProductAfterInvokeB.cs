using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductAfterInvokeB : Rule<Product>
    {
        public override IRuleResult Invoke(Product product)
        {
            product.Description = "Product Description";
            return new RuleResult { Name = "ProductRule", Result = product.Description };
        }
    }
}