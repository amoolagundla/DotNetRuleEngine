using System.Threading.Tasks;
using DotNetRuleEngine.Model;
using DotNetRuleEngine.Test.BusinessRules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class AsyncBusinessRuleTest
    {
        [TestMethod]
        public async Task TestAsyncBusinessRule()
        {
            var product = new Product();
            product.AddRules(new UpdateProductNameAsync(), new UpdateProductDescriptionAsync());
            await product.ExecuteAsync();
            Assert.AreSame("PC", product.Name);
            Assert.AreSame("Personal Computer", product.Description);
        }
    }
}