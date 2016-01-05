namespace DotNetRuleEngine.Core
{
    public interface IGeneralRule<T> where T : class, new()
    {        
        Configuration<T> Configuration { get; set;  }

        void Initialize();
    }
}