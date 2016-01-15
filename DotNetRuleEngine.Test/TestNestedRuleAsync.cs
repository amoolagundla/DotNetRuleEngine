using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.AsyncRules;
using DotNetRuleEngine.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class TestNestedRuleAsync
    {
        [TestMethod]
        public void TestAsyncNestedRules()
        {
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(new Product());

            ruleEngineExecutor.AddRules(new ProductNestedRuleAsync());

            var ruleResults = ruleEngineExecutor.ExecuteAsync().Result;

            Assert.IsNotNull(ruleResults);
            Assert.AreEqual("ProductNestedRuleAsyncC", ruleResults.FindRuleResult<ProductNestedRuleAsyncC>().Name);
        }       
    }
}
