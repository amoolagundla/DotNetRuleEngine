using System.Collections.Generic;

namespace DotNetRuleEngine.Core.Interface
{
    public interface IGeneralRule<T> where T : class, new()
    {
        T Model { get; set; }

        bool IsNested { get; }

        IDependencyResolver DependencyResolver { get; set; }

        IConfiguration<T> Configuration { get; set;  }        

        ICollection<IGeneralRule<T>> GetRules();

        void AddRules(params IGeneralRule<T>[] rules);
    }
}