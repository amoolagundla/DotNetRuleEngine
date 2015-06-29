using System;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    class ValidateName : Rule<Product>
    {        
        public override void Invoke(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                Console.WriteLine("invalid product name");
            }
        }
    }
}