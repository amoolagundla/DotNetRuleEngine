namespace DotNetRuleEngine.Core
{
    public class RuleEngineExecutor<T> : RuleEngine<T> where T : class, new()
    {        
        public RuleEngineExecutor(T instance)
        {
            Instance = instance;
        }

        public RuleEngineExecutor()
        {
        }

        public static RuleEngineExecutor<T> GetInstance(T instance)
        {
            return new RuleEngineExecutor<T>(instance);
        } 
    }
}