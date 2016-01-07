using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductNestedRule : Rule<Product>
    {
        public ProductNestedRule()
        {
            AddRules(new ProductNestedRuleA(), new ProductNestedRuleB());
        }
        public override IRuleResult Invoke(Product product)
        {
            return null;
        }
    }
}