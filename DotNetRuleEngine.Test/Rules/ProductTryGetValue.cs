using System.Collections.Generic;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using Product = DotNetRuleEngine.Test.Models.Product;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductTryGetValue : Rule<Product>
    {
        public override IRuleResult Invoke()
        {
            var descriptionList = new List<string>
            {
                TryGetValue("Description1").To<string>(),
                TryGetValue("Description2").To<string>(),
                TryGetValue("Description3").To<string>(),
                TryGetValue("Description4").To<string>()
            };

            return new RuleResult { Name = "ProductRule", Result = descriptionList };
        }
    }
}