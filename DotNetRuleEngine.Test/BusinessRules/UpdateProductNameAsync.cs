using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;

namespace DotNetRuleEngine.Test.BusinessRules
{
    class UpdateProductNameAsync : IRuleAsync<Product>
    {
        public Expression<Predicate<Product>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public async Task InvokeAsync(Product product)
        {
            product.Name = await Task.FromResult("PC");
            product.TryAdd("ProductName", product.Name);
        }
    }
}