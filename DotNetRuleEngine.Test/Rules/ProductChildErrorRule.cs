using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductChildErrorRule : Rule<Product>
    {
        public override IRuleResult Invoke()
        {
            Model.Description = "Product Description";

            return new RuleResult { Result = Model.Description, Error = new Error { Message = "Error" } };
        }
    }
}