namespace DotNetRuleEngine.Core
{
    public static class Extensions
    {
        public static T To<T>(this object @object)
        {
            return @object != null ? (T)@object : default(T);
        }
    }
}
