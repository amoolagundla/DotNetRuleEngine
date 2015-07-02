using DotNetRuleEngine.Core;
using Product = DotNetRuleEngine.Test.Models.Product;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductTryGetValue : Rule<Product>
    {
        public override IRuleResult Invoke(Product product)
        {
            product.Description = TryGetValue("Description").ToString();
            return new RuleResult { Name = "ProductRule", Result = product.Description };
        }
    }
}