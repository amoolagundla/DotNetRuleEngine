using System;
using System.Linq;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.AsyncRules;
using DotNetRuleEngine.Test.Models;
using DotNetRuleEngine.Test.Rules;
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
            var nestedRuleResults = ruleResults.First().Result.To<IRuleResult[]>(); ;
            var nestedRuleBResult = nestedRuleResults.First(n => n.Name == "ProductNestedRuleAsyncB");

            Assert.AreEqual(1, ruleResults.Length);
            Assert.AreEqual(2, nestedRuleResults.Length);
            Assert.AreEqual("ProductNestedRuleAsyncC", nestedRuleBResult.Result.To<IRuleResult[]>().First().Name);
        }
    }
}
