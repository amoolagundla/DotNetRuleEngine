using System;
using System.Collections.Generic;
using System.Linq;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core
{
    public class NullDependencyResolver : IDependencyResolver
    {
        public object GetService(Type serviceType)
        {
            return default(Type);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Enumerable.Empty<object>();
        }
    }
}
