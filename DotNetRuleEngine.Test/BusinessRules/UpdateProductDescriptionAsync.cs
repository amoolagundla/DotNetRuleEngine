using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    class UpdateProductDescriptionAsync : IRuleAsync<Product>
    {
        public Expression<Predicate<Product>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public async Task InvokeAsync(Product product)
        {
            var name = product.TryGetValue("ProductName");
            product.Description = await Task.FromResult("Personal Computer");
        }
    }
}