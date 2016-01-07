using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    public class ProductNestedParallelUpdateA : RuleAsync<Product>
    {
        public override void Initialize()
        {
            Parallel = true;

        }

        public override async Task<IRuleResult> InvokeAsync(Product type)
        {
            await Task.Delay(10);
            Debug.WriteLine("ProductNestedParallelUpdateA executed.");
            return await Task.FromResult<IRuleResult>(new RuleResult { Result = "ProductNestedParallelUpdateA executed." });
        }
    }

    public class ProductNestedParallelUpdateB : RuleAsync<Product>
    {
        public override void Initialize()
        {
            Parallel = true;
            AddRules(new ProductNestedParallelUpdateB1(), new ProductNestedParallelUpdateB2());
        }

        public override async Task<IRuleResult> InvokeAsync(Product type)
        {
            await Task.Delay(15);
            Debug.WriteLine("ProductNestedParallelUpdateB executed.");
            return await Task.FromResult<IRuleResult>(new RuleResult { Result = "ProductNestedParallelUpdateB executed." });
        }
    }

    public class ProductNestedParallelUpdateB1 : RuleAsync<Product>
    {
        public override void Initialize()
        {
            Parallel = true;

        }

        public override async Task<IRuleResult> InvokeAsync(Product type)
        {
            await Task.Delay(15);
            Debug.WriteLine("ProductNestedParallelUpdateB1 executed.");
            return await Task.FromResult<IRuleResult>(new RuleResult { Result = "ProductNestedParallelUpdateB1 executed." });
        }
    }

    public class ProductNestedParallelUpdateB2 : RuleAsync<Product>
    {
        public override void Initialize()
        {
            Parallel = true;

        }

        public override async Task<IRuleResult> InvokeAsync(Product type)
        {
            await Task.Delay(10);
            Debug.WriteLine("ProductNestedParallelUpdateB2 executed.");
            return await Task.FromResult<IRuleResult>(new RuleResult { Result = "ProductNestedParallelUpdateB2 executed." });
        }
    }

    public class ProductNestedParallelUpdateC : RuleAsync<Product>
    {
        public override void Initialize()
        {
            Parallel = true;

        }

        public override async Task<IRuleResult> InvokeAsync(Product type)
        {
            await Task.Delay(20);
            Debug.WriteLine("ProductNestedParallelUpdateC executed.");
            return await Task.FromResult<IRuleResult>(new RuleResult { Result = "ProductNestedParallelUpdateC executed." });
        }
    }
}
