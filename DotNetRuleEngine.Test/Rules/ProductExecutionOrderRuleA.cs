using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductExecutionOrderRuleA : Rule<Product>
    {
        public override void Initialize()
        {
            Configuration.ExecutionOrder = 2;
        }

        public override IRuleResult Invoke(Product type)
        {
            return new RuleResult();
        }
    }

    class ProductExecutionOrderRuleB : Rule<Product>
    {
        public override void Initialize()
        {
            Configuration.ExecutionOrder = 1;
        }

        public override IRuleResult Invoke(Product type)
        {
            return new RuleResult();
        }
    }
}
