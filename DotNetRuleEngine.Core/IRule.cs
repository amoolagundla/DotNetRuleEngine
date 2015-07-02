namespace DotNetRuleEngine.Core
{
    public interface IRule<T> : IGeneralRule<T> where T : class, new()
    {
        void BeforeInvoke();
        
        void AfterInvoke();
        
        IRuleResult Invoke(T type);
    }
}
