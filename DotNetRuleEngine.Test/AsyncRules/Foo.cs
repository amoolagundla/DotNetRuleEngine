using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class Foo
    {
        public string Name { get; set; }
        public string Phone { get; set; }
    }

    class UpdatePhone : RuleAsync<Foo>
    {
        public override Task InitializeAsync()
        {
            Parallel = true;
            return Task.FromResult<object>(null);
        }
        public override async Task<IRuleResult> InvokeAsync(Foo foo)
        {
            await Task.Delay(1000);
            TryAddAsync("Phone", Task.FromResult<object>(2063995220));
            return await Task.FromResult<IRuleResult>(null);
        }
    }


    class UpdateName : RuleAsync<Foo>
    {
        public override Task InitializeAsync()
        {
            AddRules(new UpdatePhone());
            return Task.FromResult<object>(null);

        }
        public override async Task<IRuleResult> InvokeAsync(Foo foo)
        {
            var phone = await TryGetValueAsync("Phone");
            foo.Phone = phone?.ToString();

            return await Task.FromResult<IRuleResult>(new RuleResult { Result = foo });

        }
    }
}
