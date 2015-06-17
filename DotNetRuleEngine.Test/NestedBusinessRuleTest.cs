using DotNetRuleEngine.Model;
using DotNetRuleEngine.Test.BusinessRules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class NestedBusinessRuleTest
    {
        [TestMethod]
        public void TestInnerBusinessRule()
        {
            var p = new Product();
            p.AddRules(new ProductValidation());
            p.Execute();
        }
    }
}
