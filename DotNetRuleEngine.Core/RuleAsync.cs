using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core
{
    public abstract class RuleAsync<T> : IRuleAsync<T> where T : class, new()
    {
        public bool Parallel { get; set; }

        private IList<IGeneralRule<T>> Rules { get; set; } = new List<IGeneralRule<T>>();

        public bool IsNested => Rules.Any();

        public IConfiguration<T> Configuration { get; set; } = new Configuration<T>();


        public async Task<object> TryGetValueAsync(string key)
        {
            return await RuleDataManager.GetInstance().GetValueAsync(key, Configuration);
        }

        public void TryAddAsync(string key, Task<object> value)
        {
            RuleDataManager.GetInstance().AddOrUpdateAsync(key, value, Configuration);
        }

        public ICollection<IGeneralRule<T>> GetRules()
        {
            return Rules;
        }

        public void AddRules(params IGeneralRule<T>[] rules)
        {
            Rules = rules;
        }

        public virtual async Task InitializeAsync()
        {
            await Task.FromResult<object>(null);
        }

        public virtual async Task BeforeInvokeAsync()
        {
            await Task.FromResult<object>(null);
        }

        public virtual async Task AfterInvokeAsync()
        {
            await Task.FromResult<object>(null);
        }

        public abstract Task<IRuleResult> InvokeAsync(T type);
    }
}
