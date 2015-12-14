using System.Collections.Generic;
using System.Linq;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;
using DotNetRuleEngine.Test.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class TestRule
    {
        [TestMethod]
        public void TestInvoke()
        {
            var ruleEngineExecutor = new RuleEngineExecutor<Product>(new Product());
            ruleEngineExecutor.AddRules(new ProductRule());
            var ruleResults = ruleEngineExecutor.Execute();
            Assert.AreEqual("Product Description", ruleResults.FindRuleResult<ProductRule>().Result);
        }

        [TestMethod]
        public void TestBeforeInvoke()
        {
            var ruleEngineExecutor = new RuleEngineExecutor<Product>(new Product());
            ruleEngineExecutor.AddRules(new ProductRule());
            var ruleResults = ruleEngineExecutor.Execute();

            object value;
            ruleResults.FindRuleResult("ProductRule").Data.TryGetValue("Key", out value);
            Assert.AreEqual("Value", value);
        }

        [TestMethod]
        public void TestAfterInvoke()
        {
            var ruleEngineExecutor = new RuleEngineExecutor<Product>(new Product());
            ruleEngineExecutor.AddRules(new ProductTerminateA(), new ProductTerminateB());
            var ruleResults = ruleEngineExecutor.Execute();
            var ruleResult = ruleResults.FindRuleResult("ProductRule");
            Assert.AreEqual(1, ruleResults.Length);
            Assert.IsNotNull(ruleResult);
        }

        [TestMethod]
        public void TestSkip()
        {
            var ruleEngineExecutor = new RuleEngineExecutor<Product>(new Product());
            ruleEngineExecutor.AddRules(new ProductSkip());
            var ruleResults = ruleEngineExecutor.Execute();
            Assert.IsFalse(ruleResults.Any());
        }


        [TestMethod]
        public void TestTerminate()
        {
            var ruleEngineExecutor = new RuleEngineExecutor<Product>(new Product());
            ruleEngineExecutor.AddRules(new ProductTerminateA(), new ProductTerminateB());
            var ruleResults = ruleEngineExecutor.Execute();
            Assert.AreEqual(1, ruleResults.Length);
        }


        [TestMethod]
        public void TestConstraint()
        {
            var ruleEngineExecutor = new RuleEngineExecutor<Product>(new Product());
            ruleEngineExecutor.AddRules(new ProductConstraintA(), new ProductConstraintB());
            var ruleResults = ruleEngineExecutor.Execute();
            Assert.AreEqual(1, ruleResults.Length);
        }


        [TestMethod]
        public void TestTryAddTryGetValue()
        {
            var ruleEngineExecutor = new RuleEngineExecutor<Product>(new Product());
            ruleEngineExecutor.AddRules(new ProductTryAdd(), new ProductTryGetValue());
            var ruleResults = ruleEngineExecutor.Execute().FindRuleResult("ProductRule").Result.To<List<string>>();

            Assert.AreEqual("Product Description1", ruleResults[0]);
            Assert.AreEqual("Product Description2", ruleResults[1]);
            Assert.AreEqual("Product Description3", ruleResults[2]);
            Assert.AreEqual("Product Description4", ruleResults[3]);
        }
    }
}
