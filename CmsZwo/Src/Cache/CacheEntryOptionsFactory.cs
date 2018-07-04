using System;
using Microsoft.Extensions.Caching.Memory;

namespace CmsZwo.Cache
{
	public interface ICacheEntryOptionsFactory
	{
		MemoryCacheEntryOptions Create<T>(CacheOptions<T> options = null);
	}

	public class CacheEntryOptionsFactory : ICacheEntryOptionsFactory
	{
		#region Tools

		private const int _DefaultTimeoutMinutes = 14;

		public MemoryCacheEntryOptions Create<T>(CacheOptions<T> options = null)
		{
			var timeoutMinutes =
				options?.TimeoutMinutes
				?? _DefaultTimeoutMinutes;

			var timeoutMinutesSpan = TimeSpan.FromMinutes(timeoutMinutes);

			if (options == null)
				return
					new MemoryCacheEntryOptions
					{
						SlidingExpiration = timeoutMinutesSpan
					};

			var result = new MemoryCacheEntryOptions();

			if (options.ExpirationType == CacheExpirationType.NotRemoveable)
			{
				result.Priority = CacheItemPriority.NeverRemove;
			}
			else
			{
				if (options.ExpirationType == CacheExpirationType.Sliding)
					result.SlidingExpiration = timeoutMinutesSpan;

				if (options.ExpirationType == CacheExpirationType.Absolute)
					result.AbsoluteExpirationRelativeToNow = timeoutMinutesSpan;
			}

			result.RegisterPostEvictionCallback((key, value, reason, state) =>
			{
				options.DidRemove?.Invoke(options);

				if (value is IDisposable disposable)
					disposable.Dispose();
			});

			return result;
		}

		#endregion
	}
}
