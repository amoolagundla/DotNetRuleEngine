using System.Threading.Tasks;

namespace DotNetRuleEngine.Core
{
    public interface IRuleAsync<T> : IGeneralRule<T> where T : class, new()
    {
        Task InvokeAsync(T product);
    }
}