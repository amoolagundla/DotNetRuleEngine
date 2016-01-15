using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductAfterInvokeB : Rule<Product>
    {
        public override IRuleResult Invoke()
        {
            Model.Description = "Product Description";
            return new RuleResult { Name = "ProductRule", Result = Model.Description };
        }
    }
}