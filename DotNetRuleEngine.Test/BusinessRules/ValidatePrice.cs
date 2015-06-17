using System;
using System.Linq.Expressions;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    class ValidatePrice : IRule<Product>
    {        
        public Expression<Predicate<Product>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public void Invoke(Product product)
        {
            product.TryAdd("Price", 3.99m);
        }
    }
}