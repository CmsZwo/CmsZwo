using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using ServiceStack.Redis;

namespace CmsZwo.Cache
{
	public interface IRedisGatewayService : IService
	{
		Task SetAsync<T>(
			string key,
			T obj,
			CacheOptions<T> cacheOptions = null
		);

		Task<T> GetAsync<T>(string key);
		Task<IEnumerable<T>> GetManyAsync<T>(IEnumerable<string> keys);

		Task<HashSet<string>> GetSetAsync<T>(string key);
		Task SetSetAsync(string key, IEnumerable<string> values);

		Task AddToSetAsync(string key, IEnumerable<string> items);
		Task RemoveFromSetAsync(string key, string item);

		Task RemoveAsync(string key);
		Task RemoveAsync(IEnumerable<string> keys);

		Task RemoveFromHashAsync(string hashKey, string key);

		Task<long> IncrementInHashAsync(string hashKey, string key, long current, int steps);
		Task<Dictionary<string, int>> GetHashCountersAsync(string hashKey);

		Task<long> IncrementCounterAsync(string key, long current, int steps);
		Task ResetCounterAsync(string key);

		Task SetExpirationAsync(string key, TimeSpan ts);
		Task<bool> IsCachedAsync(string key);
		Task ClearAsync();

		Task ClearIfRequiredAsync();
	}

	public class RedisGatewayService : Injectable, IRedisGatewayService
	{
		#region Inject

		[Inject]
		public IRedisClientFactory IRedisClientFactory { get; set; }

		[Inject]
		public ISettingsService ISettingsService { get; set; }

		[Inject]
		public ITimeService ITimeService { get; set; }

		#endregion

		#region IRedisCacheService

		public IRedisClient GetClient()
			=> IRedisClientFactory.GetClient();

		private CacheServiceSettings.RedisSettings _Settings
			=> ISettingsService.Get<CacheServiceSettings>()?.Redis;

		public Task SetAsync<T>(
			string key,
			T obj,
			CacheOptions<T> cacheOptions = null
		)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						client.Set(key, obj.ToJsonWithTypeInformation());

						if (cacheOptions?.ExpirationType == CacheExpirationType.Absolute)
						{
							var absoluteExpire = ITimeService.Now.AddMinutes(cacheOptions.TimeoutMinutes);
							client.ExpireEntryAt(key, absoluteExpire);
						}

						return Task.CompletedTask;
					}
				});
		}

		public Task<T> GetAsync<T>(string key)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						var json = client.Get<string>(key);
						if (!json.HasContent()) return default(T);

						var result = json.ToObjectByJson<T>();
						return result;
					}
				});
		}

		public Task<IEnumerable<T>> GetManyAsync<T>(IEnumerable<string> keys)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						var pairs = client.GetAll<string>(keys);
						if (pairs == null) return null;

						var result = pairs.Select(x => x.Value.ToObjectByJson<T>());
						return result;
					}
				});
		}

		public Task SetSetAsync(string key, IEnumerable<string> values)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						client.AddRangeToSet(key, values.ToList());
						return Task.CompletedTask;
					}
				});
		}

		public Task<HashSet<string>> GetSetAsync<T>(string key)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						var result = client.GetAllItemsFromSet(key);
						return result;
					}
				});
		}

		public Task AddToSetAsync(string key, IEnumerable<string> items)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						client.AddRangeToSet(key, items.ToList());
						return Task.CompletedTask;
					}
				});
		}

		public Task RemoveFromSetAsync(string key, string item)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						client.RemoveItemFromSet(key, item);
						return Task.CompletedTask;
					}
				});
		}

		public Task RemoveAsync(string key)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						client.Remove(key);
						return Task.CompletedTask;
					}
				});
		}

		public Task RemoveAsync(IEnumerable<string> keys)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						client.RemoveAll(keys);
						return Task.CompletedTask;
					}
				});
		}

		public Task<bool> IsCachedAsync(string key)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						return client.ContainsKey(key);
					}
				});
		}

		public Task ClearAsync()
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						client.FlushDb();
						return Task.CompletedTask;
					}
				});
		}

		public Task ClearIfRequiredAsync()
		{
			return
				Task.Run(() =>
				{
					var key = $"{nameof(RedisGatewayService)}:DataVersion";
					using (var client = GetClient())
					{
						var version = client.Get<int>(key);
						if (_Settings.DataVersion <= version)
							return;

						client.FlushDb();
						client.Set(key, _Settings.DataVersion);
					}
				});
		}

		public Task<long> IncrementInHashAsync(string hashKey, string key, long current, int steps)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						var result = client.IncrementValueInHash(hashKey, key, steps);

						if (result < current)
						{
							result = current + steps;
							client.SetEntryInHash(hashKey, key, result.ToString());
						}

						return result;
					}
				});
		}

		public Task RemoveFromHashAsync(string hashKey, string key)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						client.RemoveEntryFromHash(hashKey, key);
						return Task.CompletedTask;
					}
				});
		}

		public Task<Dictionary<string, int>> GetHashCountersAsync(string hashKey)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						var pairs = client.GetAllEntriesFromHash(hashKey);
						var result = pairs.ToDictionary(x => x.Key, x => x.Value.ToInt32());
						return result;
					}
				});
		}

		public Task<long> IncrementCounterAsync(string key, long current, int steps)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						var result = client.Increment(key, (uint)steps);
						if (result < current)
						{
							result = current + steps;
							client.Set(key, result);
						}

						return result;
					}
				});
		}

		public Task ResetCounterAsync(string key)
			=> RemoveAsync(key);

		public Task SetExpirationAsync(string key, TimeSpan ts)
		{
			return
				Task.Run(() =>
				{
					using (var client = GetClient())
					{
						client.ExpireEntryIn(key, ts);
						return Task.CompletedTask;
					}
				});
		}

		#endregion
	}
}
