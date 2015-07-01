using DotNetRuleEngine.Model;
using DotNetRuleEngine.Test.BusinessRules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class BusinessRuleTest
    {
        [TestMethod]
        public void TestBusinessRule()
        {
            var p = new Product();
            p.AddRules(new EnsureProductNameIsNotNull());
            p.Execute();
            Assert.IsTrue(!string.IsNullOrWhiteSpace(p.Name));
        }

        [TestMethod]
        public void TestBusinessRuleConstraint()
        {
            var p = new Product { Name = "PC" };
            p.AddRules(new ValidateNameWithConstraint(),
                       new ValidatePrice());
           
            p.Execute();

            var productName = p.TryGetValue("Key1");
            var productPrice = p.TryGetValue("Price");
            Assert.AreSame("Laptop", productName);
            Assert.AreEqual(3.99m, productPrice);
        }
    }
}
