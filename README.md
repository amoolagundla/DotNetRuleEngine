# DotNetRuleEngine #
## (Rule Based Software Development) ##

<br />
> DotNetRuleEngine is a ***Rule Engine*** that allows you to write sophisticated 
> rules to keep your code clean and structured. Supports both **synchronous** and **asynchronous** execution. But most importantly it does this in a very simple and elegant way.


Nuget package available at: [https://www.nuget.org/packages/DotNetRuleEngine/1.0.0](https://www.nuget.org/packages/DotNetRuleEngine/1.0.0 "DotNetRuleEngine")

**Usage:**

### **RuleEngineExecutor:** ###

#### **Synchronous API:** ####

```csharp
	Order order = new Order { Amount = 10.99m };

    RuleEngineExecutor<Order> ruleEngineExecutor = new RuleEngineExecutor<Order>(order);
    ruleEngineExecutor.AddRules(new IsValidAmount());

    //Synchronous (also supports asynchronous)
    ruleEngineExecutor.Execute();
```
#### **Order (domain model)** ####

```csharp
    public class Order
	{
		public decimal Amount { get; set; }
	}
```

#### **IsValidAmount Rule (Synchronous)** ####
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        private Stopwatch _stopwatch;

        public void Invoke(Order order)
        {
            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
        }

		//Runs before the Invoke method
        public override void BeforeInvoke()
        {
			_stopwatch = Stopwatch.StartNew();
        }

		//Runs after the Invoke method
        public override void AfterInvoke(Order order)
        {
            var totalRuntimeMs = _stopwatch.ElapsedMilliseconds;
        }
    }
```

<br />
----------
<br />

### **RuleEngine:** ###

#### **Synchronous API:** ####

```csharp
	//Create an instance of your model
    Product p = new Product();

    //Add rule(s)
	p.AddRules(new UpdateDescription());

    //Execute the rules against the product instance
	p.Execute();
```

#### **Asynchronous API:** ####

```csharp
    Product p = new Product();
    
	p.AddRules(new UpdateDescriptionAsync(),
               new UpdateNameAsync());

	await p.ExecuteAsync();
```

#### **Product (domain model)** ####

```csharp
    //Inherit your model from RuleEngine<T>
    public class Product : RuleEngine<Product>
	{
		public string Name { get; set; }
		public decimal Price { get; set; }
		public string Description { get; set; }
	}
```
 
#### **UpdateDescription Business Rule (Synchronous)** ####
    
```csharp
    //Implement IRule<T> for synchronous rules
	public class UpdateDescription : Rule<Product>
    {
        public UpdateDescription()
        {
            //Invoke method will not get called if constraint returns false
            Constraint = product => product.Description == "Desktop Computer";

            //Remaining business rules will not execute if set to true
            Terminate = true;
        }

        public void Invoke(Product product)
        {
            product.Description = "Desktop Computer";
        }
    }
```

#### **UpdateDescriptionAsync Business Rule (Asynchronous)** ####

```csharp
    //Implement IRuleAsync<T> for asynchronous rules
    public class UpdateDescriptionAsync : RuleAsync<Product>
    {
        public async Task InvokeAsync(Product product)
        {
            //Simulate API call to external service
            await Task.Delay(10);

            //Store data to share among other rules
            product.TryAdd("Description", "Desktop Computer");

            product.Description = "Desktop Computer";
        }
    }
```

#### **UpdateNameAsync Business Rule (Asynchronous)** ####

```csharp
    public class UpdateNameAsync : RuleAsync<Product>
    {
        public async Task InvokeAsync(Product product)
        {
            //Simulate API call to external service
            await Task.Delay(10);

            //Get data from another business rule
            var description = product.TryGetValue("Description");

            product.Name = "Desktop Computer" + " " + description;
        }
    }
```
