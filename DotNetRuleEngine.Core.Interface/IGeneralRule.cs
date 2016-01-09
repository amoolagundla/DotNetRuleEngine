using System.Collections.Generic;

namespace DotNetRuleEngine.Core.Interface
{
    public interface IGeneralRule<T> where T : class, new()
    { 
        bool IsNested { get; }

        IConfiguration<T> Configuration { get; set;  }        

        ICollection<IGeneralRule<T>> GetRules();

        void AddRules(params IGeneralRule<T>[] rules);
    }
}