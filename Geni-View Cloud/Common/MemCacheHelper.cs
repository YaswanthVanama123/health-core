using GeniView.Cloud.Models;
using GeniView.Data;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RenityArtemis.Web.Common
{
    /// <summary>
    /// Wraps IMemoryCache for caching command results.
    /// IMemoryCache is injected via DI (registered in Program.cs as AddMemoryCache).
    /// </summary>
    public class MemCacheHelper
    {
        private readonly IMemoryCache _cache;

        public MemCacheHelper(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Set data to cache, default expired time is 5 seconds.
        /// </summary>
        public void SetCache<T>(string key, T dataObject, int expiredSecond = 5)
        {
            var options = new MemoryCacheEntryOptions();
            if (expiredSecond >= 0)
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiredSecond);
            }
            _cache.Set(key, dataObject, options);
        }

        /// <summary>
        /// Get data by key. Returns default(T) if not found.
        /// </summary>
        public T? GetCache<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return value;
        }

        public List<CommandResult> GetLogRateResult(List<string> ids)
        {
            return GetResultsFromCache("LogRateResult", ids);
        }

        public List<CommandResult> GetOTAResult(List<string> ids)
        {
            return GetResultsFromCache("OTAResult", ids);
        }

        public List<CommandResult> GetNTPResult(List<string> ids)
        {
            return GetResultsFromCache("NTPResult", ids);
        }

        private List<CommandResult> GetResultsFromCache(string cacheKey, List<string> ids)
        {
            var ret = GetCache<ConcurrentDictionary<string, CommandResult>>(cacheKey);
            var result = new List<CommandResult>();

            if (ids != null && ids.Count > 0)
            {
                foreach (var item in ids)
                {
                    if (ret != null && ret.TryGetValue(item, out CommandResult? log))
                    {
                        result.Add(log);
                    }
                    else
                    {
                        var unknown = new CommandResult(item)
                        {
                            Guid = Guid.Empty,
                            DateTimeUTC = DateTime.MinValue.ToString()
                        };
                        result.Add(unknown);
                    }
                }
            }
            else
            {
                if (ret != null)
                {
                    result.AddRange(ret.Values);
                }
            }

            return result;
        }
    }
}
