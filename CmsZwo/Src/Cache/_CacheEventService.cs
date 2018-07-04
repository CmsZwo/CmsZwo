using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CmsZwo
{
	public interface ICacheEventService : IService
	{
		Task Register(CacheEvent cacheEvent, string cacheKey);
		Task Register(IEnumerable<CacheEvent> cacheEvents, string cacheKey);

		Task Trigger(CacheEvent cacheEvent);
		Task Trigger(IEnumerable<CacheEvent> cacheEvents);
	}

	public class CacheEventService : Injectable, ICacheEventService
	{
		#region Inject

		[Inject]
		public ICacheService ICacheService { get; set; }

		#endregion

		#region Tools

		private string GetSetKey(CacheEvent cacheEvent)
			=> $"{nameof(CacheEventService)}:{cacheEvent.Name}";

		#endregion

		#region ICacheEventService

		public async Task Register(CacheEvent cacheEvent, string cacheKey)
		{
			if (cacheEvent == null) return;

			if (cacheEvent.Name.HasContent() != true)
				throw new ArgumentException($"{nameof(cacheEvent)} must have content.");

			if (!cacheKey.HasContent())
				throw new ArgumentException($"{nameof(cacheKey)} must have content.");

			await ICacheService.AddToSetAsync(cacheEvent.Name, cacheKey);
		}

		public async Task Register(IEnumerable<CacheEvent> cacheEvents, string cacheKey)
		{
			foreach (var cacheEvent in cacheEvents.Safe())
				await Register(cacheEvent, cacheKey);
		}

		public async Task Trigger(CacheEvent cacheEvent)
		{
			var keys = await ICacheService.GetSetAsync(cacheEvent.Name);
			await ICacheService.RemoveAsync(keys);
		}

		public async Task Trigger(IEnumerable<CacheEvent> cacheEvents)
		{
			foreach (var cacheEvent in cacheEvents.Safe())
				await Trigger(cacheEvent);
		}

		#endregion
	}
}
