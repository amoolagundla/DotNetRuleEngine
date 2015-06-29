using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    class ValidatePrice : Rule<Product>
    {        
        public override void Invoke(Product product)
        {
            product.TryAdd("Price", 3.99m);
        }
    }
}