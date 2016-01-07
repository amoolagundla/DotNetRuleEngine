using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.AsyncRules;
using DotNetRuleEngine.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    /// <summary>
    /// Test Parallel Rule
    /// </summary>
    [TestClass]
    public class TestParallelRule
    {
        [TestMethod]
        public void TestParallelRules()
        {
            var product = new Product();
            var engineExecutor = new RuleEngineExecutor<Product>(product);
            var ruleEngineExecutor = engineExecutor;

            ruleEngineExecutor.AddRules(
                new ProductParallelUpdateNameRuleAsync(),
                new ProductParallelUpdateDescriptionRuleAsync(),
                new ProductParallelUpdatePriceRuleAsync());

            var ruleResults = ruleEngineExecutor.ExecuteAsync().Result;

            Assert.IsNotNull(ruleResults);
            Assert.AreEqual("Product", product.Name);
            Assert.AreEqual(0.0m, product.Price);
            Assert.AreEqual("Description", product.Description);
        }

        [TestMethod]
        public void TestNestedParallelRules()
        {
            var product = new Product();
            var engineExecutor = new RuleEngineExecutor<Product>(product);
            var ruleEngineExecutor = engineExecutor;

            ruleEngineExecutor.AddRules(
                new ProductNestedParallelUpdateA(),
                new ProductNestedParallelUpdateB(),
                new ProductNestedParallelUpdateC());

            var ruleResults = ruleEngineExecutor.ExecuteAsync().Result;
        }
    }
}
