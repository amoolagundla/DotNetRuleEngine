using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductNestedRuleB : NestedRule<Product>
    {
        public override IRuleResult Invoke(Product product)
        {
            Instance = product;
            AddRules(new ProductNestedRuleC());
            var ruleResults = Execute();

            return new RuleResult { Name = "ProductNestedRuleB", Result = ruleResults };
        }
    }
}