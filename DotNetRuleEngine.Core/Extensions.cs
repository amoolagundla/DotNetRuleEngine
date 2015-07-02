namespace DotNetRuleEngine.Core
{
    public static class Extensions
    {
        public static T To<T>(this object @object)
        {           
            return (T) @object;
        }
    }
}
