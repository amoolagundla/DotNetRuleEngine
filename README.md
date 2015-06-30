# DotNetRuleEngine #
## (Rule Based Software Development) ##

<br />
> DotNetRuleEngine is a ***Rule Engine*** that allows you to write sophisticated 
> rules to keep your code clean and structured. Supports both **synchronous** and **asynchronous** execution. But most importantly it does this in a very simple and elegant way.


```csharp
    PM> Install-Package DotNetRuleEngine
```
Nuget package available at: [DotNetRuleEngine](https://www.nuget.org/packages/DotNetRuleEngine/1.1.0 "DotNetRuleEngine")


#### **RuleEngineExecutor API:** ####

##### **Synchronous:** #####

```csharp
	Order order = new Order { Amount = 10.99m };

    RuleEngineExecutor<Order> ruleEngineExecutor = new RuleEngineExecutor<Order>(order);
    ruleEngineExecutor.AddRules(new IsValidAmount());
   
    ruleEngineExecutor.Execute();
```


##### **Asynchronous:** #####

```csharp
	Order order = new Order { Amount = 10.99m };

    RuleEngineExecutor<Order> ruleEngineExecutor = new RuleEngineExecutor<Order>(order);
    ruleEngineExecutor.AddRules(new IsValidAmountAsync());
    
    ruleEngineExecutor.ExecuteAsync();
```

###### **Order (domain model)** ######

```csharp
    public class Order
	{
		public decimal Amount { get; set; }
	}
```

###### **IsValidAmount *Rule* (Synchronous)** ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        public override void Invoke(Order order)
        {
            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
        }
    }
```

###### **IsValidAmountAsync *Rule* (Asynchronous)** ######
```csharp
    public class IsValidAmountAsync : RuleAsync<Order>
    {   
        public override async Task Invoke(Order order)
        {
            //Simulate API call to external service
			await Task.Delay(10);

            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
        }
    }
```

<br />


>The difference between RuleEngineExecutor and RuleEngine, when you use RuleEngine, your model *must* inherit from RuleEngine. RuleEngineExecutor doesn't have this requirement.

#### **RuleEngine API** ####

##### **Synchronous** #####

```csharp
	//Create an instance of your model
    Product p = new Product();

    //Add rule(s)
	p.AddRules(new UpdateDescription());

    //Execute the rules against the product instance
	p.Execute();
```

##### **Asynchronous** #####

```csharp
    Product p = new Product();
    
	p.AddRules(new UpdateDescriptionAsync(),
               new UpdateNameAsync());

	await p.ExecuteAsync();
```

###### **Product (domain model)** ######

```csharp
    //Inherit your model from RuleEngine<T>
    public class Product : RuleEngine<Product>
	{
		public string Name { get; set; }
		public decimal Price { get; set; }
		public string Description { get; set; }
	}
```
 
###### **UpdateDescription *Rule* (Synchronous)** ######
    
```csharp
    //Inherit from Rule<T> for synchronous rules
	public class UpdateDescription : Rule<Product>
    {
        public override void Invoke(Product product)
        {
            product.Description = "Desktop Computer";
        }
    }
```

###### **UpdateDescriptionAsync *Rule* (Asynchronous)** ######

```csharp
    //Inherit from RuleAsync<T> for asynchronous rules
    public class UpdateDescriptionAsync : RuleAsync<Product>
    {
        public override async Task InvokeAsync(Product product)
        {
            //Simulate API call to external service
            await Task.Delay(10);

            product.Description = "Desktop Computer";
        }
    }
```

###### **UpdateNameAsync *Rule* (Asynchronous)** ######

```csharp
    public class UpdateNameAsync : RuleAsync<Product>
    {
        public override async Task InvokeAsync(Product product)
        {
            //Simulate API call to external service
            await Task.Delay(10);

            product.Name = "Dell Inspiron";
        }
    }
```

----------

#### Features ####

##### NestedRule/NestedRuleAsync #####

Rules can be nested. Derive from NestedRule or NestedRuleAsync to implement nested rules.

###### Example ######
```csharp
    public class IsValidAmount : NestedRule<Order>
    {   
        public override void Invoke(Order order)
        {
            AddRules(new AmountGreaterThan1000());
			Execute();
        }
    }
```
```csharp
    public class AmountGreaterThan1000 : Rule<Order>
    {   
        public override void Invoke(Order order)
        {
            if (order.Amount > 1000)
            {
                order.Shipping = 5;
            }
        }
    }
```

##### Before/After Invoke #####
They're invoked before and after the Invoke method.

###### Example ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        private Stopwatch _stopwatch;

        public override void Invoke(Order order)
        {
            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
        }

		//Runs before Invoke method
        public override void BeforeInvoke()
        {
			_stopwatch = Stopwatch.StartNew();
        }

		//Runs after Invoke method
        public override void AfterInvoke()
        {
            var totalRuntimeMs = _stopwatch.ElapsedMilliseconds;
        }
    }
```

##### Skip #####
Mark any rule to be skipped by setting Skip = true. *Must be set before Invoke method executed.*

###### Example ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        public override void Invoke(Order order)
        {
            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
        }

        public override void BeforeInvoke()
        {
			Skip = true;
        }
    }
```

##### Terminate #####
You can terminate execution of the remaining rules by setting Terminate = true

###### Example ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        public override void Invoke(Order order)
        {
            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
        }

        public override void AfterInvoke()
        {
			Terminate = true;
        }
    }
```

##### Constraint #####
If Constraint property evaluated to false, Invoke method will not be executed. *Must be set before Invoke method executed.*

###### Example ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        public override void Invoke(Order order)
        {
            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
        }

        public override void BeforeInvoke()
        {
			Constraint = order => order.Amount > 1000;
        }
    }
```

##### TryAdd/TryGet #####
Share data between rules.

###### Example ######
```csharp
    public class UpdateDescription : Rule<Product>
    {
        public override void Invoke(Product product)
        {
            //Store data to share with the other rules
            product.TryAdd("Description", "Desktop Computer");
        }
    }
```
```csharp
    public class UpdateName : Rule<Product>
    {
        public override void Invoke(Product product)
        {
            //Rerieve data from another rule
            var description = product.TryGetValue("Description");
        }
    }
```
