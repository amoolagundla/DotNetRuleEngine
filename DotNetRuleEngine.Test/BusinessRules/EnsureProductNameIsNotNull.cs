using System;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    public class EnsureProductNameIsNotNull : Rule<Product>
    {
        public EnsureProductNameIsNotNull()
        {
            Constraint = product => product.Name == "Desktop Computer";
        }

        public override void Invoke(Product type)
        {
            if (string.IsNullOrWhiteSpace(type.Name))
            {
                throw new Exception();
            }
        }
    }
}