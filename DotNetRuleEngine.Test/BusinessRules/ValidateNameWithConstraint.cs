using System;
using System.Linq.Expressions;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    class ValidateNameWithConstraint : IRule<Product>
    {
        public ValidateNameWithConstraint()
        {
            Constraint = b => b.Name == "Laptop";
        }
        public Expression<Predicate<Product>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public void Invoke(Product product)
        {
            product.TryAdd("Name", "Laptop");
        }
    }
}