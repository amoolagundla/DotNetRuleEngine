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
            var ruleEngineExecutor = new RuleEngineExecutor<Product>(new Product());

            ruleEngineExecutor.AddRules(new ProductNestedRuleAsync());

            var ruleResults = ruleEngineExecutor.ExecuteAsync().Result;
            var nestedRuleResult = ruleResults.FindNestedRuleResult("ProductNestedRuleAsyncC");

            Assert.IsNotNull(ruleResults);
            Assert.AreEqual("ProductNestedRuleAsyncC", nestedRuleResult.Name);
        }       
    }
}
