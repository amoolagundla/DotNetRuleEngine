# DotNetRuleEngine #
## (Rule Based Software Development) ##
*S.O.L.I.D COMPLIANT*
<br />
> DotNetRuleEngine allows you to write your business logic as series of rules to keep your code clean and structured. Supports both **synchronous** and **asynchronous** execution and it is **S.O.L.I.D** compliant.


```csharp
    PM> Install-Package DotNetRuleEngine
```
Nuget package available at: [DotNetRuleEngine](https://www.nuget.org/packages/DotNetRuleEngine/1.4.3 "DotNetRuleEngine")


#### Model

```csharp
    public class Order
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public bool FreeShipping { get; set; }
    }
```

#### Create rule

```csharp
    public class AmountGreaterThan50Dollars: Rule<Order>
    {   
        public override IRuleResult Invoke()
        {
            if (Model.Amount > 50.0m)
            {
                Model.FreeShipping = true;
            }
            
            return null;
        }
    }
```

#### Invoke rule(s)

```csharp
    
    Order order = new Order { Amount = 79.99m };

    var ruleResults = RuleEngine<Order>.GetInstance(order)
        .ApplyRules(new AmountGreaterThan50Dollars())
        .Execute()
```
