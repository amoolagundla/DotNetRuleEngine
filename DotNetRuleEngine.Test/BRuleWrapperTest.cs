using System;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;
using DotNetRuleEngine.Test.BusinessRules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class BRuleWrapperTest
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestBRuleWrapperException()
        {
            var bRuleWrapper = new RuleEngineExecutor<Order>();
            bRuleWrapper.AddRules(new IsValidAmount());

            bRuleWrapper.Execute();
        }

        [TestMethod]
        public void TestBRuleWrapper()
        {
            var order = new Order { Amount = 1 };
            var bRuleWrapper = new RuleEngineExecutor<Order>(order);
            bRuleWrapper.AddRules(new IsValidAmount());
            bRuleWrapper.Execute();

            Assert.AreEqual(1, order.Amount);
        }

        [TestMethod]
        public void TestInstanceProperty()
        {
            var order = new Order { Amount = 1 };
            var bRuleWrapper = new RuleEngineExecutor<Order> { Instance = order };
            bRuleWrapper.AddRules(new IsValidAmount());
            bRuleWrapper.Execute();

            Assert.AreEqual(1, order.Amount);
        }
    }
}
