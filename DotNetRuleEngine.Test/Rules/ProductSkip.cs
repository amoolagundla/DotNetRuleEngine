using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductSkip : Rule<Product>
    {
        public override void BeforeInvoke()
        {
            Configuration.Skip = true;
        }

        public override IRuleResult Invoke(Product product)
        {
            product.Description = "Product Description";
            return new RuleResult { Name = "ProductRule", Result = product.Description };
        }
    }
}