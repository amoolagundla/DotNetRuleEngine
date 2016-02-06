using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductAfterInvokeA : Rule<Product>
    {
        public override void AfterInvoke()
        {
            Configuration.Terminate = true;
        }

        public override IRuleResult Invoke()
        {
            Model.Description = "Product Description";
            return new RuleResult { Name = "ProductRule", Result = Model.Description };
        }
    }
}