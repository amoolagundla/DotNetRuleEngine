using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductNestedErrorRule : NestedRule<Product>
    {
        public ProductNestedErrorRule()
        {
            AddChildRules(new ProductChildErrorRule(), new ProductNestedRuleA());
        }
        public override IRuleResult Invoke(Product product)
        {
            return null;
        }
    }
}