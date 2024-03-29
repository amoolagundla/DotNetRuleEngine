﻿using System;
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

        public const int DefaultTimeoutInMs = 15000;

        public async Task AddOrUpdateAsync<T>(string key, Task<object> value, IConfiguration<T> configuration)
        {
            var ruleengineId = GetRuleengineId(configuration);

            var keyPair = BuildKey(key, ruleengineId);
            await Task.FromResult(AsyncData.Value.AddOrUpdate(keyPair.First(), v => value, (k, v) => value));
        }

        public async Task<object> GetValueAsync<T>(string key, IConfiguration<T> configuration, int timeoutInMs = DefaultTimeoutInMs)
        {
            var timeout = DateTime.Now.AddMilliseconds(timeoutInMs);
            var ruleengineId = GetRuleengineId(configuration);
            var keyPair = BuildKey(key, ruleengineId);

            while (DateTime.Now < timeout)
            {
                Task<object> value;
                AsyncData.Value.TryGetValue(keyPair.First(), out value);

                if (value != null)
                {
                    return await value;
                }
            }

            throw new TimeoutException($"Unable to get {key}");
        }

        public void AddOrUpdate<T>(string key, object value, IConfiguration<T> configuration)
        {
            var ruleengineId = GetRuleengineId(configuration);
            var keyPair = BuildKey(key, ruleengineId);

            Data.Value.AddOrUpdate(keyPair.First(), v => value, (k, v) => value);
        }

        public object GetValue<T>(string key, IConfiguration<T> configuration, int timeoutInMs = DefaultTimeoutInMs)
        {
            var timeout = DateTime.Now.AddMilliseconds(timeoutInMs);
            var ruleengineId = GetRuleengineId(configuration);
            var keyPair = BuildKey(key, ruleengineId);

            while (DateTime.Now < timeout)
            {
                object value;
                Data.Value.TryGetValue(keyPair.First(), out value);

                if (value != null)
                {
                    return value;
                }
            }

            throw new TimeoutException($"Unable to get {key}");
        }

        public static RuleDataManager GetInstance() => DataManager.Value;

        private static string[] BuildKey(string key, string ruleengineId) => new[] { string.Join("_", ruleengineId, key), key };

        private static string GetRuleengineId<T>(IConfiguration<T> configuration) => ((RuleEngineConfiguration<T>)configuration).RuleEngineId.ToString();
    }
}
