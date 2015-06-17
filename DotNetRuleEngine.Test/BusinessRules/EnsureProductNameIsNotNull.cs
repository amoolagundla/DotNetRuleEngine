using System;
using System.Linq.Expressions;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    public class EnsureProductNameIsNotNull : IRule<Product>
    {
        public EnsureProductNameIsNotNull()
        {
            Constraint = product => product.Name == "Desktop Computer";
        }
        public Expression<Predicate<Product>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public void Invoke(Product type)
        {
            if (string.IsNullOrWhiteSpace(type.Name))
            {
                throw new Exception();
            }
        }
    }
}