using Microsoft.Extensions.Caching.Memory;

namespace CmsZwo.Cache.Memory
{
	public interface IMemoryCacheFactory : IService
	{
		MemoryCache Create();
	}

	public class MemoryCacheFactory : IMemoryCacheFactory
	{
		#region IMemoryCacheFactory

		public MemoryCache Create()
			=> new MemoryCache(new MemoryCacheOptions());

		#endregion
	}
}
