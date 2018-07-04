using System;
using System.Collections.Generic;

namespace CmsZwo.Cache
{
	public class CacheOptions<T>
	{
		public CacheOptions(
			CacheExpirationType expirationType = CacheExpirationType.Sliding,
			int timeoutMinutes = 14
		)
		{
			ExpirationType = expirationType;
			TimeoutMinutes = timeoutMinutes;
		}

		public CacheExpirationType ExpirationType { get; set; }
			= CacheExpirationType.Sliding;

		public int TimeoutMinutes { get; set; }

		public Func<T, IEnumerable<CacheEvent>> CreateCacheEvents { get; set; }
		public Action<CacheOptions<T>> DidRemove { get; set; }
	}

	public enum CacheExpirationType
	{
		Sliding = 1,
		Absolute = 2,
		NotRemoveable = 4
	}
}
