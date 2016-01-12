using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Interface;

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


        public async Task AddOrUpdateAsync<T>(string key, Task<object> value, IConfiguration<T> configuration)
        {
            var ruleengineId = GetRuleengineId(configuration);

            var keyPair = BuildKey<T>(key, ruleengineId);
            await Task.FromResult(AsyncData.Value.AddOrUpdate(keyPair.First(), v => value, (k, v) => value));
        }

        public async Task<object> GetValueAsync<T>(string key, IConfiguration<T> configuration, int timeoutInMs = 30000)
        {
            var timeout = DateTime.Now.AddMilliseconds(timeoutInMs);
            return await GetValueAsync(key, configuration, timeout);
        }

        public async Task<object> GetValueAsync<T>(string key, IConfiguration<T> configuration,
            DateTime timeout)
        {
            var ruleengineId = GetRuleengineId(configuration);
            var keyPair = BuildKey<T>(key, ruleengineId);

            Task<object> value = null;
            

            while (value == null && DateTime.Now < timeout)
            {
                AsyncData.Value.TryGetValue(keyPair.First(), out value);
            }

            return value != null ? await value : null;
        }

        public void AddOrUpdate<T>(string key, object value, IConfiguration<T> configuration)
        {
            var ruleengineId = GetRuleengineId(configuration);
            var keyPair = BuildKey<T>(key, ruleengineId);

            Data.Value.AddOrUpdate(keyPair.First(), v => value, (k, v) => value);
        }

        public object GetValue<T>(string key, IConfiguration<T> configuration, int timeoutInMs = 30000)
        {
            var timeout = DateTime.Now.AddMilliseconds(timeoutInMs);
           return GetValue(key, configuration, timeout);
        }

        public object GetValue<T>(string key, IConfiguration<T> configuration, DateTime timeout)
        {
            var ruleengineId = GetRuleengineId(configuration);
            var keyPair = BuildKey<T>(key, ruleengineId);

            object value = null;
            
            while (value == null && DateTime.Now < timeout)
            {
                Data.Value.TryGetValue(keyPair.First(), out value);
            }

            return null;
        }

        public static RuleDataManager GetInstance()
        {
            return DataManager.Value;
        }

        private static string[] BuildKey<T>(string key, string ruleengineId)
        {
            return new[]
            {
                string.Join("_", ruleengineId, key),
                key
            };
        }

        private static string GetRuleengineId<T>(IConfiguration<T> configuration)
        {
            var ruleengineId = ((RuleEngineConfiguration<T>)configuration).RuleEngineId.ToString();
            return ruleengineId;
        }
    }
}
