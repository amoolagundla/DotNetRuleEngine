# DotNetRuleEngine #
## (Rule Based Software Development) ##
*S.O.L.I.D COMPLIANT*
<br />
> DotNetRuleEngine is a ***Rule Engine*** that allows you to write sophisticated 
> rules to keep your code clean and structured. Supports both **synchronous** and **asynchronous** execution. And most importantly it does this in a simple and an elegant way.


```csharp
    PM> Install-Package DotNetRuleEngine
```
Nuget package available at: [DotNetRuleEngine](https://www.nuget.org/packages/DotNetRuleEngine/1.4.1 "DotNetRuleEngine")


#### **RuleEngineExecutor API:** ####

**Examples:**

###### **Order (domain model)** ######

```csharp
    public class Order
	{
		public decimal Amount { get; set; }
	}
```

##### **Synchronous Example:** #####

```csharp

    //New up your model
    Order order = new Order { Amount = 10.99m };

    //Add rules to run against the model instance
    var ruleEngineExecutor = new RuleEngineExecutor<Order>(order);
    ruleEngineExecutor.AddRules(new IsValidAmount());
   
    //Execute the rules
    IRuleResult[] ruleResults = ruleEngineExecutor.Execute();
```

###### **IsValidAmount *Rule* (Synchronous)** ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        public override IRuleResult Invoke(Order order)
        {
            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
            
            return null;
        }
    }
```


##### **Asynchronous Example:** #####

```csharp
    
    //New up your model
    Order order = new Order { Amount = 10.99m };

    //Add rules to run against the model instance
    var ruleEngineExecutor = new RuleEngineExecutor<Order>(order);
    ruleEngineExecutor.AddRules(new IsValidAmountAsync());
    
    //Execute the rules
    IRuleResult[] ruleResults = await ruleEngineExecutor.ExecuteAsync();
```

###### **IsValidAmountAsync *Rule* (Asynchronous)** ######
```csharp
    public class IsValidAmountAsync : RuleAsync<Order>
    {   
        public override async Task<IRuleResult> Invoke(Order order)
        {
            //Simulate API call to external service
			await Task.Delay(10);

            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
            
            return Task.FromResult<object>(null);
        }
    }
```

###### **IRuleResult** ######
The return value of Rule/RuleAsync.

<table>
<tr>
<td>Name</td>
<td>Used to store name of the rule. (Defaults to class name of the rule).</td>
</tr>
<tr>
<td>Result</td>
<td>Used to store outcome of the rule.</td>
</tr>
<tr>
<td>Data</td>
<td>Used to store any arbitrary data.</td>
</tr>
</table>

```csharp
    public interface IRuleResult
    {
        string Name { get; set; }
        object Result { get; set; }
        Dictionary<string, object> Data { get; set; }
    }
```

**RuleResult is the implementation of IRuleResult*

<br />


>The difference between RuleEngineExecutor and RuleEngine, when you use RuleEngine, your model *must* inherit from RuleEngine. RuleEngineExecutor doesn't have this requirement.

#### **RuleEngine API** ####

**Examples:**

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

##### **Synchronous Example:** #####

```csharp
    
    //New up your model
    Product p = new Product();

    //Add rules to run against the model instance
    p.AddRules(new UpdateDescription());

    //Execute the rules. (Null rule results will be ignored by Execute method)
    IRuleResult[] ruleResults = p.Execute();
```

###### **UpdateDescription *Rule* (Synchronous)** ######
    
```csharp
    //Inherit from Rule<T> for synchronous rules
	public class UpdateDescription : Rule<Product>
    {
        public override IRuleResult Invoke(Product product)
        {
            product.Description = "Desktop Computer";

			return null; 
        }
    }
```

##### **Asynchronous Example:** #####

```csharp

    //New up your model
    Product p = new Product();
    
    //Add rules to run against the model instance
    p.AddRules(new UpdateDescriptionAsync());

    //Execute the rules. (Null rule results will be ignored by Execute method)
    IRuleResult[] ruleResults = await p.ExecuteAsync();
```

###### **UpdateDescriptionAsync *Rule* (Asynchronous)** ######

```csharp
    //Inherit from RuleAsync<T> for asynchronous rules
    public class UpdateDescriptionAsync : RuleAsync<Product>
    {
        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            //Simulate API call to external service
            await Task.Delay(10);

            product.Description = "Desktop Computer";

            return Task.FromResult<object>(null);
        }
    }
```

----------

#### Features ####

##### Execution Order #####

(*in progress...*)

Rules can specify ```ExecutionOrder``` (asc to desc) rather than getting executed in the order they've been added to ```AddRules``` method.

*if both* ```Parallel``` *and* ```ExecutionOrder``` *specified for async rule, then the parallelization would be ignored.*


In the proceeding example, there are two rules. The order of execution would be ```ValidateProductAmount```, and then the ```ValidateProductName```

###### Example ######
```csharp
    public class ValidateProductAmount : RuleAsync<Order>
    {   
        public override async BeforeInvoke()
        {
            ExecutionOrder = 1
        }  

        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            return Task.FromResult<object>(null);
        }      
    }
```

```csharp
    public class ValidateProductName : RuleAsync<Order>
    {   
        public override async BeforeInvoke()
        {
            ExecutionOrder = 2
        }  

        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            return Task.FromResult<object>(null);
        }      
    }    
```


##### Parallelization #####

Async rules can be specified as ```Parallel``` to be executed in parallel. 

If ```Parallel``` not specified, async rules executed in the order they are added to the AddRules method unless the ```ExecutionOrder``` specified.

###### Example ######
```csharp
    public class IsValidAmount : RuleAsync<Order>
    {   
 		public override void BeforeInvoke()
        {
			Parallel = true;
        }

        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }

            return Task.FromResult<object>(null);
        }      
    }
```

##### NestedRule/NestedRuleAsync #####

Rules can be nested. Derive from ```NestedRule``` or ```NestedRuleAsync``` to implement nested rules.

###### Example ######
```csharp
    public class IsValidAmount : NestedRule<Order>
    {   
        public override IRuleResult Invoke(Order order)
        {
            Instance = order;
            AddRules(new AmountGreaterThan1000());
			ruleResults = Execute();

            return new RuleResult { Result = ruleResult };
        }
    }
```
```csharp
    public class AmountGreaterThan1000 : Rule<Order>
    {   
        public override IRuleResult Invoke(Order order)
        {
            if (order.Amount > 1000)
            {
                order.Shipping = 5;
            }
            
            return null;
        }
    }
```

##### Before/After Invoke #####
Executed before and after the Invoke method.

###### Example ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        private Stopwatch _stopwatch;

        public override IRuleResult Invoke(Order order)
        {
            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }

            return null;
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
Marks the rule to be skipped. Invoke method will not be executed. *Must be set before Invoke method executed.*

###### Example ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        public override IRuleResult Invoke(Order order)
        {
            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }

            return null;
        }

        public override void BeforeInvoke()
        {
			Skip = true;
        }
    }
```

##### Terminate #####
Terminates execution of the remaining rules.

###### Example ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        public override IRuleResult Invoke(Order order)
        {
            if (order.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }

            return null;
        }

        public override void AfterInvoke()
        {
			Terminate = true;
        }
    }
```

##### Constraint #####
If evaluated to false, Invoke method will not be executed. *Must be set before Invoke method executed.*

###### Example ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        public override IRuleResult Invoke(Order order)
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
        public override IRuleResult Invoke(Product product)
        {
            //Store data to share with the other rules
            TryAdd("Description", "Desktop Computer");

            return null;
        }
    }
```
```csharp
    public class UpdateName : Rule<Product>
    {
        public override IRuleResult Invoke(Product product)
        {
            //Retrieve data from another rule
            var description = TryGetValue("Description").To<string>();
 
            return null;
        }
    }
```

##### TryAddAsync/TryGetAsync #####
Share data between async rules.


###### Example ######
```csharp
    public class UpdateDescription : AsyncRule<Product>
    {
        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            TryAddAsync("Description", Task.FromResult<object>(product.Description));

            return Task.FromResult<object>(null);
        }  
    }
```
```csharp
    public class UpdateName : AsyncRule<Product>
    {
        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            var description = TryGetValueAsync("Description").Result.To<string>();

            return Task.FromResult<object>(null);
        }  
    }
```
