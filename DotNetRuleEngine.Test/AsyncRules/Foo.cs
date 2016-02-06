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
        public override async Task<IRuleResult> InvokeAsync()
        {
            await Task.Delay(1000);
            await TryAddAsync("Phone", Task.FromResult<object>(2063995220));
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
        public override async Task<IRuleResult> InvokeAsync()
        {
            var phone = await TryGetValueAsync("Phone");
            Model.Phone = phone?.ToString();

            return await Task.FromResult<IRuleResult>(new RuleResult { Result = Model });

        }
    }
}
