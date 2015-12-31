using System;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductRuleError : Rule<Product>
    {
        public override IRuleResult Invoke(Product product)
        {
            product.Description = "Product Description";

            return new RuleResult { Error = new Error { Message = "Error", Exception = new Exception() } };
        }
    }
}