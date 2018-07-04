using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using CmsZwo.Cache;
using CmsZwo.Cache.Redis;
using CmsZwo.Cache.Memory;

namespace CmsZwo
{
	public interface ICacheService : ICacheDelegate, IService { }

	public class CacheServiceSettings
	{
		public RedisSettings Redis { get; set; }
			= new RedisSettings();

		public class RedisSettings
		{
			public bool Enabled { get; set; }
			public int DataVersion { get; set; } = 1;
			public string ConnectionString { get; set; }
			public string SyncChannel { get; set; } = "_Sync";
		}
	}

	public class CacheService : Injectable, ICacheService
	{
		#region Inject

		[Inject]
		public ISettingsService ISettingsService { get; set; }

		[Inject]
		public IRedisCacheDelegate IRedisCacheDelegate { get; set; }

		[Inject]
		public IMemoryCacheDelegate IMemoryCacheDelegate { get; set; }

		#endregion

		#region Delegate

		private ICacheDelegate _ICacheDelegate;
		private ICacheDelegate ICacheDelegate
		{
			get
			{
				if (_ICacheDelegate == null)
				{
					var redisSettings =
						ISettingsService
							.Get<CacheServiceSettings>()
							?.Redis;

					if (redisSettings != null)
						_ICacheDelegate = IRedisCacheDelegate;
					else
						_ICacheDelegate = IMemoryCacheDelegate;

				}

				return _ICacheDelegate;
			}
		}

		#endregion

		#region IDataCacheDelegate

		public Task AddToSetAsync(string key, string item)
			=> ICacheDelegate.AddToSetAsync(key, item);

		public Task AddToSetAsync(string key, IEnumerable<string> items)
			=> ICacheDelegate.AddToSetAsync(key, items);

		public Task ClearAsync()
			=> ICacheDelegate.ClearAsync();

		public Task<T> GetAsync<T>(string key, Func<T> factory = null, CacheOptions<T> options = null)
			=> ICacheDelegate.GetAsync(key, factory, options);

		public Task<IEnumerable<T>> GetManyAsync<T>(IEnumerable<string> keys)
			=> ICacheDelegate.GetManyAsync<T>(keys);

		public Task<HashSet<string>> GetSetAsync(string key, Func<IEnumerable<string>> factory)
			=> ICacheDelegate.GetSetAsync(key, factory);

		public Task<ulong> IncrementCounterAsync(string key, Func<ulong> factory, ulong steps = 1)
			=> ICacheDelegate.IncrementCounterAsync(key, factory, steps);

		public Task<bool> IsCachedAsync(string key)
			=> ICacheDelegate.IsCachedAsync(key);

		public Task RemoveAsync(string key)
			=> ICacheDelegate.RemoveAsync(key);

		public Task RemoveAsync(IEnumerable<string> keys)
			=> ICacheDelegate.RemoveAsync(keys);

		public Task RemoveFromSetAsync(string key, string item)
			=> ICacheDelegate.RemoveFromSetAsync(key, item);

		public Task RemoveFromSetAsync(string key, IEnumerable<string> items)
			=> ICacheDelegate.RemoveFromSetAsync(key, items);

		public Task ResetCounterAsync(string key)
			=> ICacheDelegate.ResetCounterAsync(key);

		public Task SetAsync<T>(string key, T obj, CacheOptions<T> options = null)
			=> ICacheDelegate.SetAsync(key, obj, options);

		#endregion
	}
}
