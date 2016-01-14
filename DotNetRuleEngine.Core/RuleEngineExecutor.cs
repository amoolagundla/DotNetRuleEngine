namespace DotNetRuleEngine.Core
{
    public sealed class RuleEngineExecutor<T> : RuleEngine<T> where T : class, new()
    {        
        public RuleEngineExecutor(T instance)
        {
            SetInstance(instance);
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