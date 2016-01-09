using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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


        public void AddOrUpdateAsync<T>(string key, Task<object> value, IConfiguration<T> configuration)
        {
            var ruleengineId = GetRuleengineId(configuration);

            key = BuildKey<T>(key, ruleengineId);
            AsyncData.Value.AddOrUpdate(key, v => value, (k, v) => value);

            Debug.WriteLine($"{ruleengineId} : {AsyncData.Value.Count}");
        }

        public Task<object> GetValueAsync<T>(string key, IConfiguration<T> configuration)
        {            
            var ruleengineId = GetRuleengineId(configuration);

            key = BuildKey<T>(key, ruleengineId);

            Task<object> value;
            return AsyncData.Value.TryGetValue(key, out value) ? value : Task.FromResult<object>(null);
        }

        public void AddOrUpdate<T>(string key, object value, IConfiguration<T> configuration)
        {
            var ruleengineId = GetRuleengineId(configuration);

            key = BuildKey<T>(key, ruleengineId);
            Data.Value.AddOrUpdate(key, v => value, (k, v) => value);
        }

        public object GetValue<T>(string key, IConfiguration<T> configuration)
        {
            var ruleengineId = GetRuleengineId(configuration);

            key = BuildKey<T>(key, ruleengineId);

            object value;
            return Data.Value.TryGetValue(key, out value) ? value : null;
        }

        public static RuleDataManager GetInstance()
        {
            return DataManager.Value;
        }

        private static string BuildKey<T>(string key, string ruleengineId)
        {
            return string.Join("_", ruleengineId, key);
        }

        private static string GetRuleengineId<T>(IConfiguration<T> configuration)
        {
            var ruleengineId = ((RuleEngineConfiguration<T>)configuration).RuleEngineId.ToString();
            return ruleengineId;
        }
    }
}
