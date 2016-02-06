using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductNestedRuleA : Rule<Product>
    {
        public override void Initialize()
        {
            Configuration.ExecutionOrder = 2;
        }

        public override IRuleResult Invoke()
        {
            Model.Description = "Product Description";

            return new RuleResult { Name = "ProductNestedRuleA", Result = Model.Description };
        }
    }
}