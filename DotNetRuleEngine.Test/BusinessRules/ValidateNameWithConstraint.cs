using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    class ValidateNameWithConstraint : Rule<Product>
    {
        public override void BeforeInvoke()
        {
            Constraint = b => b.Name == "Laptop";
            TryAdd("Key1", "Laptop");
        }

        public override void Invoke(Product product)
        {
            product.TryAdd("Name", "Laptop");
        }
    }
}