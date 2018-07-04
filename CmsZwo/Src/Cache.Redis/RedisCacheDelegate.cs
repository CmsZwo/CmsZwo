using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using CmsZwo.Cache.Memory;

namespace CmsZwo.Cache.Redis
{
	public interface IRedisCacheDelegate : ICacheDelegate { }

	public class RedisCacheDelegate : MemoryCacheDelegate, IRedisCacheDelegate
	{
		#region Inject

		[Inject]
		public IRedisGatewayService IRedisGatewayService { get; set; }

		[Inject]
		public IRedisCacheBus IRedisCacheBus { get; set; }

		#endregion

		#region Tools

		private readonly bool _UseCacheAside = true;

		#endregion

		#region Overrides

		public override async Task ResetCounterAsync(string key)
		{
			await base.ResetCounterAsync(key);
			await IRedisGatewayService.ResetCounterAsync(key);
			await IRedisCacheBus.RemoveAsync(key);
		}

		public override async Task<T> GetAsync<T>(
			string key,
			Func<T> factory = null,
			CacheOptions<T> options = null
		)
		{
			var cacheObj =
				_UseCacheAside
					? IMemoryCacheService.Get<T>(key)
					: default(T);

			if (cacheObj is NotFound) return default(T);

			var result = cacheObj == null ? default(T) : cacheObj;
			if (result != null) return result;

			result = await IRedisGatewayService.GetAsync<T>(key);

			if (result == null)
			{
				if (factory != null)
					result = factory();
			}

			if (result == null)
			{
				IMemoryCacheService.Set(key, NotFound.Shared);
				return default(T);
			}

			if (_UseCacheAside)
				IMemoryCacheService.Set(key, result, options);

			await IRedisGatewayService.SetAsync(key, result);
			return result;
		}

		public override async Task<IEnumerable<T>> GetManyAsync<T>(IEnumerable<string> keys)
		{
			var result = await base.GetManyAsync<T>(keys);

			if (!result.HasContent())
				result = await IRedisGatewayService.GetManyAsync<T>(keys);

			return result;
		}

		public override async Task SetAsync<T>(
			string key,
			T obj,
			CacheOptions<T> options = null
		)
		{
			await base.SetAsync(key, obj, options);

			var oldRedis = await IRedisGatewayService.GetAsync<T>(key);
			await IRedisGatewayService.SetAsync(key, obj);

			if (oldRedis == null)
				return;

			var oldMd5 = oldRedis.ToJsonMd5();
			var newMd5 = obj.ToJsonMd5();

			if (oldMd5 == newMd5)
				return;

			await IRedisCacheBus.RemoveAsync(key);
		}

		public override async Task RemoveAsync(IEnumerable<string> keys)
		{
			await base.RemoveAsync(keys);

			if (!keys.HasContent())
				return;

			await IRedisGatewayService.RemoveAsync(keys);
			await IRedisCacheBus.RemoveManyAsync(keys);
		}

		public override async Task<bool> IsCachedAsync(string key)
		{
			var result = await base.IsCachedAsync(key);
			if (result) return result;

			return await IRedisGatewayService.IsCachedAsync(key);
		}

		public override async Task ClearAsync()
		{
			await base.ClearAsync();
			await IRedisGatewayService.ClearAsync();
			await IRedisCacheBus.ClearAsync();
		}

		public override async Task<HashSet<string>> GetSetAsync(
			string key,
			Func<IEnumerable<string>> factory = null
		)
		{
			var result = IMemoryCacheService.Get<HashSet<string>>(key);
			if (result != null) return result;

			var values = await IRedisGatewayService.GetSetAsync<string>(key);

			if (!values.HasContent())
				values = factory.Invoke().ToHashSet();

			await IRedisGatewayService.SetSetAsync(key, values);
			result = values.ToHashSet();

			if (_UseCacheAside)
			{
				var entryOptions = new CacheOptions<HashSet<string>>(expirationType: CacheExpirationType.NotRemoveable);
				IMemoryCacheService.Set(key, result, entryOptions);
			}

			return result;
		}

		public override async Task AddToSetAsync(string key, IEnumerable<string> items)
		{
			await base.AddToSetAsync(key, items);

			await IRedisGatewayService.AddToSetAsync(key, items);
			await IRedisCacheBus.AddToSetAsync(key, items);
		}

		public override async Task RemoveFromSetAsync(string key, IEnumerable<string> items)
		{
			await base.RemoveFromSetAsync(key, items);

			foreach (var item in items)
			{
				await IRedisGatewayService.RemoveFromSetAsync(key, item);
				await IRedisCacheBus.RemoveFromSetAsync(key, item);
			}
		}

		#endregion
	}

	public class NotFound
	{
		public static readonly NotFound Shared
			= new NotFound();
	}
}
