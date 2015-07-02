using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductConstraintB : Rule<Product>
    {
        public override void BeforeInvoke()
        {
            Constraint = product => product.Description == "";
        }

        public override IRuleResult Invoke(Product product)
        {
            product.Description = "Product Description";
            return new RuleResult { Name = "ProductRule", Result = product.Description };
        }
    }
}