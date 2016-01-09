using System.Linq;
using DotNetRuleEngine.Core.Interface;
using System.Collections.Generic;

namespace DotNetRuleEngine.Core
{
    public abstract class Rule<T> : IRule<T> where T : class, new()
    {
        private IList<IGeneralRule<T>> Rules { get; set; } = new List<IGeneralRule<T>>();

        public bool IsNested => Rules.Any();

        public IConfiguration<T> Configuration { get; set; } = new Configuration<T>();

        public object TryGetValue(string key)
        {
            return RuleDataManager.GetInstance().GetValue(key, Configuration);
        }

        public void TryAdd(string key, object value)
        {
            RuleDataManager.GetInstance().AddOrUpdate(key, value, Configuration);
        }

        public IReadOnlyCollection<IGeneralRule<T>> GetRules()
        {
            return (IReadOnlyCollection<IGeneralRule<T>>)Rules;
        }

        public void AddRules(params IGeneralRule<T>[] rules)
        {
            Rules = rules;
        }

        public virtual void Initialize()
        {
        }
        public virtual void BeforeInvoke()
        {
        }

        public virtual void AfterInvoke()
        {
        }

        public abstract IRuleResult Invoke(T type);
    }
}
