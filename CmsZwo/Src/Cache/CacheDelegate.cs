using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using CmsZwo.Cache.Memory;

namespace CmsZwo.Cache
{
	public interface ICacheDelegate : IService
	{
		Task<ulong> IncrementCounterAsync(
			string key,
			Func<ulong> factory,
			ulong steps = 1);

		Task ResetCounterAsync(string key);

		Task<T> GetAsync<T>(
			string key,
			Func<T> factory = null,
			CacheOptions<T> options = null
		);

		Task<IEnumerable<T>> GetManyAsync<T>(IEnumerable<string> keys);

		Task SetAsync<T>(
			string key,
			T obj,
			CacheOptions<T> options = null
		);

		Task RemoveAsync(string key);
		Task RemoveAsync(IEnumerable<string> keys);

		Task<bool> IsCachedAsync(string key);
		Task ClearAsync();

		Task<HashSet<string>> GetSetAsync(
			string key,
			Func<IEnumerable<string>> factory = null
		);

		Task AddToSetAsync(string key, string item);
		Task AddToSetAsync(string key, IEnumerable<string> items);

		Task RemoveFromSetAsync(string key, string item);
		Task RemoveFromSetAsync(string key, IEnumerable<string> items);
	}

	public abstract class CacheDelegate : Injectable, ICacheDelegate
	{
		#region Inject

		[Inject]
		public IMemoryCacheService IMemoryCacheService { get; set; }

		#endregion

		#region Locks

		private const int _ExpirationMinutes = 14;

		private ConcurrentDictionary<string, object> _Locks =
			new ConcurrentDictionary<string, object>();

		private object GetLock(string key)
			=> _Locks.GetOrAdd(key, x => new object());

		private void RemoveLock(string key)
			=> _Locks.TryRemove(key, out var trash);

		protected T GetOrCreateWithLock<T>(
			string key,
			Func<T> get,
			Func<T> factory,
			CacheOptions<T> options
			)
		{
			var result = get();

			if (result != null)
				return result;

			var lockObj = GetLock(key);
			lock (lockObj)
			{
				result = get();
				if (result != null)
					return result;

				result = factory();
			}
			RemoveLock(key);

			return result;
		}

		#endregion

		#region IMemoryCacheDelegate

		public virtual Task<ulong> IncrementCounterAsync(
			string key,
			Func<ulong> factory,
			ulong steps = 1
			)
		{
			var value = IMemoryCacheService.GetOrCreate(key, x => factory());
			value += steps;
			IMemoryCacheService.Set(key, value);
			return Task.FromResult(value);
		}

		public virtual Task ResetCounterAsync(string key)
		{
			IMemoryCacheService.Set(key, 0ul);
			return Task.CompletedTask;
		}

		public virtual Task<T> GetAsync<T>(
			string key,
			Func<T> factory = null,
			CacheOptions<T> options = null
			)
		{
			var result =
				GetOrCreateWithLock(
					key,
					() => IMemoryCacheService.Get<T>(key),
					() =>
					{
						if (factory == null)
							return default(T);

						var _result = factory();
						IMemoryCacheService.Set(key, _result, options);
						return _result;
					},
					options
				);

			return Task.FromResult(result);
		}

		public virtual Task<IEnumerable<T>> GetManyAsync<T>(IEnumerable<string> keys)
		{
			var result = new List<T>();

			foreach (var key in keys)
			{
				var value = IMemoryCacheService.Get<T>(key);
				if (value == null) continue;
				result.Add(value);
			}

			return Task.FromResult(result as IEnumerable<T>);
		}

		public virtual Task SetAsync<T>(
			string key,
			T obj,
			CacheOptions<T> options = null
			)
		{
			IMemoryCacheService.Set(key, obj, options);
			return Task.CompletedTask;
		}

		public virtual Task RemoveAsync(string key)
		{
			IMemoryCacheService.Remove(key);
			return Task.CompletedTask;
		}

		public virtual Task RemoveAsync(IEnumerable<string> keys)
		{
			foreach (var key in keys)
				IMemoryCacheService.Remove(key);
			return Task.CompletedTask;
		}

		public virtual Task<bool> IsCachedAsync(string key)
		{
			var result = IMemoryCacheService.Get(key) != null;
			return Task.FromResult(result);
		}

		public virtual Task ClearAsync()
		{
			IMemoryCacheService.Clear();
			return Task.CompletedTask;
		}

		public virtual Task<HashSet<string>> GetSetAsync(
			string key,
			Func<IEnumerable<string>> factory = null
			)
		{
			var result =
				GetOrCreateWithLock(
					key,
					() => IMemoryCacheService.Get<HashSet<string>>(key),
					() =>
					{
						var items =
							factory != null
								? factory()
								: new string[] { };

						var _result = items.ToHashSet();

						var entryOptions = new CacheOptions<HashSet<string>>(expirationType: CacheExpirationType.NotRemoveable);

						IMemoryCacheService.Set(key, _result, entryOptions);

						return _result;
					},
					null
				);

			return Task.FromResult(result);
		}

		public virtual async Task AddToSetAsync(string key, string item)
			=> await AddToSetAsync(key, new[] { item });

		public virtual async Task AddToSetAsync(string key, IEnumerable<string> items)
		{
			var hashSet = await GetSetAsync(key, () => items);
			foreach (var item in items)
				hashSet.Add(item);
		}

		public virtual async Task RemoveFromSetAsync(string key, string item)
			=> await RemoveFromSetAsync(key, new[] { item });

		public virtual async Task RemoveFromSetAsync(string key, IEnumerable<string> items)
		{
			var hashSet = await GetSetAsync(key, () => new string[] { });
			foreach (var item in items)
				hashSet.Remove(item);
		}

		#endregion
	}
}
