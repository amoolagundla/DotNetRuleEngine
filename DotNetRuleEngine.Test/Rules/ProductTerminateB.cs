using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using Product = DotNetRuleEngine.Test.Models.Product;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductTerminateB : Rule<Product>
    {
        public override IRuleResult Invoke()
        {
            Model.Description = "Product Description";
            return new RuleResult { Name = "ProductRule", Result = Model.Description };
        }
    }
}