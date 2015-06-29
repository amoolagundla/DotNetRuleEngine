using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    class ProductValidation : NestedRule<Product>
    {
        public override void Invoke(Product product)
        {
            Instance = product;
            AddRules(new ValidateName(),
                new ValidateDescription());

            Execute();
        }
    }
}