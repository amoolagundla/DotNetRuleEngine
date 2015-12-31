using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductNestedErrorRule : NestedRule<Product>
    {
        public override IRuleResult Invoke(Product product)
        {
            SetInstance(product);
            AddRules(new ProductChildErrorRule(), new ProductNestedRuleA());
            var ruleResults = Execute();

            return new RuleResult { Result = ruleResults };
        }
    }
}