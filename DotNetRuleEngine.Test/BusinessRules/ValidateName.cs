using System;
using System.Linq.Expressions;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    class ValidateName : IRule<Product>
    {        
        public Expression<Predicate<Product>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public void Invoke(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                Console.WriteLine("invalid product name");
            }
        }
    }
}