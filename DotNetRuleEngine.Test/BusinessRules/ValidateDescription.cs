using System;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    public class ValidateDescription : Rule<Product>
    {
        public override void Invoke(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Description))
            {
                Console.WriteLine("invalid product description");
            }
        }
    }
}