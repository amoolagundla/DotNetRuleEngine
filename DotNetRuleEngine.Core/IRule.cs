namespace DotNetRuleEngine.Core
{
    public interface IRule<T> : IGeneralRule<T> where T : class, new()
    {
        void Invoke(T type);
    }
}
