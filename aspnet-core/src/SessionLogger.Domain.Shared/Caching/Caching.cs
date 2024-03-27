using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Caching;
using System.Linq;

namespace SessionLogger.Domain
{
    public static class Caching
    {
        static readonly ObjectCache Cache = MemoryCache.Default;

        /// <summary>
        /// Retrieve cached item
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Name of cached item</param>
        /// <returns>Cached item as type</returns>
        public static T Get<T>(string key) //where T : class
        {
            try
            {
                return (T)Cache[key];
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Clears each cached object
        /// </summary>
        public static void ClearCache()
        {
            List<string> cacheKeys = Cache.Select(kvp => kvp.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                Cache.Remove(cacheKey);
            }
        }

        public static void Remove(string cacheKey)
        {
            Cache.Remove(cacheKey);
            Cache.Remove("_dep_" + cacheKey);
        }

        public static void CreateCachedObject<T>(string key, T objectToStore, int timeoutMinutes = 0, List<string> dependantCacheKeys = null, List<string> dependantFiles = null) //where T : class
        {
            if (objectToStore == null)
            {
                return;
            }

            if (key.IndexOf("-") > -1)
            {
                // auto depend on -* for keys like product-somecode
                var starCode = key.Substring(0, key.IndexOf("-")) + "-*";
                if (dependantCacheKeys == null)
                {
                    dependantCacheKeys = new List<string>();
                }
                dependantCacheKeys.AddIfNotContains(starCode);
            }

            var policy = new CacheItemPolicy();
            policy.Priority = CacheItemPriority.Default;

            // timeout
            if (timeoutMinutes == 0)
            {
                timeoutMinutes = 60;
            }

            policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(timeoutMinutes);

            // file dependencies
            if (dependantFiles != null)
            {
                policy.ChangeMonitors.Add(new HostFileChangeMonitor(dependantFiles));
            }

            List<string> dependancyKeys = new List<string>();

            // create monitoring keys if not present
            // create monitoring keys if not present
            if (dependantCacheKeys != null)
            {
                foreach (string cacheKey in dependantCacheKeys)
                {
                    string strNewKey = "_dep_" + cacheKey;
                    if (!Cache.Contains(strNewKey))
                    {
                        Cache.Add(strNewKey, "x", new CacheItemPolicy());
                    }
                    dependancyKeys.Add(strNewKey);
                }
            }

            if (dependancyKeys.Count > 0)
            {
                policy.ChangeMonitors.Add(Cache.CreateCacheEntryChangeMonitor(dependancyKeys));
            }


            if (Cache.Contains(key)) {
                Cache.Remove(key);
            }

            Cache.Add(key, objectToStore, policy);

            if (Cache.Contains("_dep_" + key))
            {
                Cache.Remove("_dep_" + key);
            }

            var policy2 = new CacheItemPolicy();
            policy2.ChangeMonitors.Add(Cache.CreateCacheEntryChangeMonitor(new List<string> {key}));
            Cache.Add("_dep_" + key, "x", policy2);  // for dependency monitoring
        }
    }
}
