﻿using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductRuleAsync : RuleAsync<Product>
    {
        public override Task BeforeInvokeAsync()
        {
            TryAddAsync("Description", Task.FromResult<object>("Description"));

            return Task.FromResult<object>(null);
        }

        public override async Task<IRuleResult> InvokeAsync()
        {
            var description = TryGetValueAsync("Description").Result.To<string>();
            Model.Description = $"Product {description}";            

            return await RuleResult.CreateAsync(new RuleResult
            {
                Name = "ProductRule",
                Result = Model.Description,
                Data = { { "Description", description } }
            });
        }
    }
}