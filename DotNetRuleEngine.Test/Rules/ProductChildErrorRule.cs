using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductChildErrorRule : Rule<Product>
    {
        public override IRuleResult Invoke(Product product)
        {
            product.Description = "Product Description";

            return new RuleResult { Result = product.Description, Error = new Error { Message = "Error" } };
        }
    }
}