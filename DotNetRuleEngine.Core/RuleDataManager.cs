using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;


namespace DotNetRuleEngine.Core
{
    internal class RuleDataManager
    {
        
        private static readonly Lazy<RuleDataManager> DataManager = new Lazy<RuleDataManager>(() => new RuleDataManager(), true);

        private RuleDataManager()
        {
        }

        private Lazy<ConcurrentDictionary<string, Task<object>>> AsyncData { get; } = new Lazy<ConcurrentDictionary<string, Task<object>>>(
            () => new ConcurrentDictionary<string, Task<object>>(), true);

        private Lazy<ConcurrentDictionary<string, object>> Data { get; } = new Lazy<ConcurrentDictionary<string, object>>(
           () => new ConcurrentDictionary<string, object>(), true);


        public const int DefaultTimeoutInMs = 15000;

        public async Task AddOrUpdateAsync<T>(string key, Task<object> value)
        {

            await Task.FromResult(AsyncData.Value.AddOrUpdate(key, v => value, (k, v) => value));
        }

        public async Task<object> GetValueAsync<T>(string key, int timeoutInMs = DefaultTimeoutInMs)
        {
            var timeout = DateTime.Now.AddMilliseconds(timeoutInMs);

            while (DateTime.Now < timeout)
            {
                Task<object> value;
                AsyncData.Value.TryGetValue(key, out value);

                if (value != null)
                {
                    return await value;
                }
            }

            throw new TimeoutException($"Unable to get {key}");
        }

        public void AddOrUpdate<T>(string key, object value)
        {
            Data.Value.AddOrUpdate(key, v => value, (k, v) => value);
        }

        public object GetValue<T>(string key, int timeoutInMs = DefaultTimeoutInMs)
        {
            var timeout = DateTime.Now.AddMilliseconds(timeoutInMs);

            while (DateTime.Now < timeout)
            {
                object value;
                Data.Value.TryGetValue(key, out value);

                if (value != null)
                {
                    return value;
                }
            }

            throw new TimeoutException($"Unable to get {key}");
        }

        public static RuleDataManager GetInstance()
        {
            return DataManager.Value;
        }
    }
}
