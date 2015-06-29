using System;
using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Model;
using DotNetRuleEngine.Test.BusinessRules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class RuleEngineExecutorTest
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestRuleEngineExecutorException()
        {
            var ruleEngineExecutor = new RuleEngineExecutor<Order>();
            ruleEngineExecutor.AddRules(new IsValidAmount());
            ruleEngineExecutor.Execute();
        }

        [TestMethod]
        public void TestBRuleWrapper()
        {
            var order = new Order { Amount = 1 };
            var ruleEngineExecutor = new RuleEngineExecutor<Order>(order);
            ruleEngineExecutor.AddRules(new IsValidAmount());
            
            ruleEngineExecutor.Execute();

            Assert.AreEqual(1, order.Amount);
        }

        [TestMethod]
        public void TestRuleEngineExecutorEvents()
        {
            var order = new Order { Amount = 1 };
            var ruleEngineExecutor = new RuleEngineExecutor<Order>(order);
            ruleEngineExecutor.AddRules(new IsValidAmountAsync());
            
            Task.FromResult(ruleEngineExecutor.ExecuteAsync());

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
