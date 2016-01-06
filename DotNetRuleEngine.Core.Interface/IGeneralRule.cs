namespace DotNetRuleEngine.Core.Interface
{
    public interface IGeneralRule<T> where T : class, new()
    {        
        IConfiguration<T> Configuration { get; set;  }

        void Initialize();
    }
}