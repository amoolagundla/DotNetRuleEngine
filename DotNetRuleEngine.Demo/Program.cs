using System;
using DotNetRuleEngine.Demo.BusinessRules;
using DotNetRuleEngine.Model;


namespace DotNetRuleEngine.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var product = new Product();

            product.AddRules(new ChangeNameAsync(),
                             new UpdateDescriptionAsync(),
                             new ChangePriceAsync());

            product.ExecuteAsync().Wait();

            Console.WriteLine(product.Name);
            Console.WriteLine(product.Price);
            Console.WriteLine(product.Description);


            Console.ReadKey();
        }
    }
}
