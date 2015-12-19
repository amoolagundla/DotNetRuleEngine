using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductExecutionOrderRuleB : Rule<Product>
    {
        public override void SetExecutionOrder()
        {
            ExecutionOrder = 1;
        }

        public override IRuleResult Invoke(Product type)
        {
            return new RuleResult();
        }
    }
}