﻿using System.Diagnostics;
using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductParallelUpdateNameRuleAsync : RuleAsync<Product>
    {
        public override Task InitializeAsync()
        {
            Parallel = true;

            return Task.FromResult<object>(null);
        }

        public override async Task<IRuleResult> InvokeAsync()
        {
            await Task.Delay(15);
            Model.Name = "Product";
            Debug.WriteLine("ProductParallelUpdateNameRuleAsync");

            return await Task.FromResult<IRuleResult>(null);
        }
    }
}