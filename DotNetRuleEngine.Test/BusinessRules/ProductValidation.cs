using System;
using System.Linq.Expressions;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    class ProductValidation : RuleEngine<Product>, IRule<Product>
    {
        public Expression<Predicate<Product>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public void Invoke(Product product)
        {
            Instance = product;
            AddRules(new ValidateName(),
                new ValidateDescription());

            Execute();
        }
    }
}