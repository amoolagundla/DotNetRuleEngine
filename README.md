# DotNetRuleEngine #
## (Rule Based Software Development) ##
*S.O.L.I.D COMPLIANT*
<br />
> DotNetRuleEngine allows you to write your business logic as series of rules to keep your code clean and structured. Supports both **synchronous** and **asynchronous** execution and it is **S.O.L.I.D** compliant.


```csharp
    PM> Install-Package DotNetRuleEngine
```
Nuget package available at: [DotNetRuleEngine](https://www.nuget.org/packages/DotNetRuleEngine/2.0.0 "DotNetRuleEngine")


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

    Order order = new Order { Amount = 10.99m };

    var ruleResults = RuleEngine<Order>.GetInstance(order)
        .ApplyRules(new IsValidAmount())
        .Execute()
```

###### **IsValidAmount *Rule* (Synchronous)** ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        public override IRuleResult Invoke()
        {
            if (Model.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
            
            return null;
        }
    }
```


##### **Asynchronous Example:** #####

```csharp
    
    Order order = new Order { Amount = 10.99m };

    var ruleResults = RuleEngine<Order>.GetInstance(order)
        .ApplyRules(new IsValidAmount())
        .ExecuteAsync()
```

###### **IsValidAmountAsync *Rule* (Asynchronous)** ######
```csharp
    public class IsValidAmountAsync : RuleAsync<Order>
    {   
        public override async Task<IRuleResult> Invoke()
        {
            //Simulate API call to external service
			await Task.Delay(10);

            if (Model.Amount <= 0.0m)
            {
                throw new InvalidOperationException();
            }
            
            return await RuleResult.Null()
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
<td>Error</td>
<td>Used to store error details.</td>
</tr>
</table>


**RuleResult is the implementation of IRuleResult*

<br />

----------

#### Features ####

##### Initialize (a.k.a Lazy Constructor) #####
Always executed. ( Most of the configuration options must be initialized in ```Initialize``` or ```ctor```. Specifically ```Skip```, ```Constraint```, and ```ExecutionOrder```.

###### Example ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        public override IRuleResult Invoke(Order order)
        {
            if (order.Amount > 50.0m)
            {
                order.FreeShipping = true; 
            }

            return null;
        }

        public override void Initialize()
        {
			Skip = true;
        }
    }
```

##### Execution Order #####

Rules can specify ```ExecutionOrder``` (ordered by asc to desc) rather than getting executed in the order they've been added to the rules.

*Must be set in Initialize method or ctor.*

*if both* ```Parallel``` *and* ```ExecutionOrder``` *specified (applies to async rules only), then the parallelization will get ignored.*

In the proceeding example, there are two rules. The order of execution would be ```ValidateProductAmount```, and then the ```ValidateProductName```

###### Example ######
```csharp
    public class ValidateProductAmount : RuleAsync<Order>
    {   
        public override Initialize()
        {
            Configuration.ExecutionOrder = 1
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
        public override Initialize()
        {
            Configuration.ExecutionOrder = 2
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
 		public override void Initialize()
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
	    public IsValidAmount()
		{
			AddChildRules(new AmountGreaterThan1000());
		}

        public override IRuleResult Invoke(Order order)
        {
            return null;
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
Executes before and after the ```Invoke``` method. It won't be executed if ```Skip``` set to true or ```Constraint``` predicate returns false.

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
Marks the rule to be skipped. Invoke method will not be executed. *Must be set in Initialize method or ctor.*

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

        public override void Initialize()
        {
			Configuration.Skip = true;
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

        public override void Initialize()
        {
			Configuration.Terminate = true;
        }
    }
```

##### Constraint #####
If evaluated to false, Invoke method will not be executed. *Must be set in Initialize method or ctor.*

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

        public override void Initialize()
        {
			Configuration.Constraint = order => order.Amount > 1000;
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

##### IRuleResult #####
```RuleResult``` can be returned from Rule/RuleAsync. (If no result needs to be returned, then simply return ```null``` from ```Rule```, or  ```Task.FromResult<object>(null);``` from ```AsynRule```) 

###### Example ######
```csharp
    public class IsValidAmount : Rule<Order>
    {   
        ProductRepository _productRepository;

		RuleResult ruleResult = new RuleResult();

		public IsValidAmount(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public override Invoke(Product product)
        {            
			try
            {
				var productAvailability = _productRepository.GetAvailability(product);
				ruleResult.Result = productAvailability;
            } 
            catch(Exception exception)
            {
				ruleResult.Error = new Error { Message = "Could't get details", Exception = exception };
            }

            return ruleResult;
        }      
    }
```


```csharp
	void Main()
	{
		Product product = new Product()

		//Get all rule results
		var results = RuleEngineExecutor<Foo>.GetInstance(product)
			.ApplyRules(new IsValidAmount())
			.Execute();
			
        //Get specific rule result
        var isValidAmountResult = results.FindRuleResult<IsValidAmount>();

        //Get errored rules.
		results.GetErrors();
	}
```
