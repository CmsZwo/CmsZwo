using System;

using Microsoft.Extensions.Caching.Memory;

namespace CmsZwo.Cache.Memory
{
	public interface IMemoryCacheService : IService
	{
		T GetOrCreate<T>(string key, Func<ICacheEntry, T> factory);

		T Set<T>(string key, T value, CacheOptions<T> options = null);

		T Get<T>(string key);
		object Get(string key);

		void Remove(string key);

		void Clear();
	}

	public class MemoryCacheService : Injectable, IMemoryCacheService
	{
		#region Inject

		[Inject]
		public IMemoryCacheFactory IMemoryCacheFactory { get; set; }

		#endregion

		#region Construct

		private readonly ICacheEntryOptionsFactory _ICacheEntryOptionsFactory
			= new CacheEntryOptionsFactory();

		private IMemoryCache _IMemoryCache;
		private IMemoryCache IMemoryCache
		{
			get
			{
				if (_IMemoryCache == null)
					CreateCache();

				return _IMemoryCache;
			}
		}

		private void CreateCache()
		{
			if (_IMemoryCache != null)
				_IMemoryCache.Dispose();

			_IMemoryCache = IMemoryCacheFactory.Create();
		}

		#endregion

		#region IMemoryCacheService

		public T GetOrCreate<T>(string key, Func<ICacheEntry, T> factory)
			=> IMemoryCache.GetOrCreate(key, factory);

		public T Set<T>(string key, T value, CacheOptions<T> options = null)
		{
			if (options != null)
			{
				var entryOptions = _ICacheEntryOptionsFactory.Create(options);
				return IMemoryCache.Set(key, value, entryOptions);
			}

			return IMemoryCache.Set(key, value);
		}

		public T Get<T>(string key)
			=> IMemoryCache.Get<T>(key);

		public object Get(string key)
			=> IMemoryCache.Get(key);

		public void Remove(string key)
			=> IMemoryCache.Remove(key);

		public void Clear()
			=> CreateCache();

		#endregion
	}
}
