using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Demo.BusinessRules
{
    public class ChangePriceAsync : IRuleAsync<Product>
    {
        public Expression<Predicate<Product>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public async Task InvokeAsync(Product product)
        {
            await Task.Delay(100);
            product.Price = 12.99m;
        }
    }
}