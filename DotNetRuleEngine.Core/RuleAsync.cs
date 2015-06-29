using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DotNetRuleEngine.Core
{
    public abstract class RuleAsync<T> : IRuleAsync<T> where T : class, new()
    {

        public Expression<Predicate<T>> Constraint
        {
            get { return null; }
            set { }
        }

        public bool Terminate { get; set; }


        public bool Skip { get; set; }


        public virtual async Task BeforeInvokeAsync()
        {
            await Task.Run(() => { });
        }

        public virtual async Task AfterInvokeAsync()
        {
            await Task.Run(() => { });
        }

        public abstract Task InvokeAsync(T type);
    }
}