using System.Linq;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;
using DotNetRuleEngine.Test.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class TestNestedRule
    {
        [TestMethod]
        public void TestNestedRules()
        {
            var ruleEngineExecutor = new RuleEngineExecutor<Product>(new Product());
            ruleEngineExecutor.AddRules(new ProductNestedRule());
            var ruleResults = ruleEngineExecutor.Execute();
            var nestedRuleResults = ruleResults.First().Result.To<IRuleResult[]>();
            var nestedRuleBResult = nestedRuleResults.First(n => n.Name == "ProductNestedRuleB");

            Assert.AreEqual(1, ruleResults.Length);
            Assert.AreEqual(2, nestedRuleResults.Length);
            Assert.AreEqual("ProductNestedRuleC", nestedRuleBResult.Result.To<IRuleResult[]>().First().Name);
        }
    }
}
