using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductNestedRule : NestedRule<Product>
    {
        public override IRuleResult Invoke(Product product)
        {
            SetInstance(product);
            AddRules(new ProductNestedRuleA(), new ProductNestedRuleB());
            var ruleResults = Execute();

            return new RuleResult { Name = "ProductNestedRule", Result = ruleResults };
        }
    }
}