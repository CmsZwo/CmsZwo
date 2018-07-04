using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace CmsZwo.Cache.Tests
{
	public class CacheEntryOptionsFactoryTests
	{
		[Fact]
		public void Create_Default()
		{
			var service = new CacheEntryOptionsFactory();
			var result = service.Create<string>();
			Assert.Equal(14, result.SlidingExpiration.Value.Minutes);
		}

		[Fact]
		public void Create_Sliding()
		{
			var service = new CacheEntryOptionsFactory();
			var result = service.Create(new CacheOptions<string>
			{
				TimeoutMinutes = 5
			});
			Assert.Equal(5, result.SlidingExpiration.Value.Minutes);
		}

		[Fact]
		public void Create_Absolute()
		{
			var service = new CacheEntryOptionsFactory();
			var result = service.Create(new CacheOptions<string>(CacheExpirationType.Absolute, 5));
			Assert.Equal(5, result.AbsoluteExpirationRelativeToNow.Value.Minutes);
		}

		[Fact]
		public void Create_NotRemoveable()
		{
			var service = new CacheEntryOptionsFactory();
			var result = service.Create(new CacheOptions<string>(CacheExpirationType.NotRemoveable));
			Assert.Equal(CacheItemPriority.NeverRemove, result.Priority);
		}

		[Fact]
		public void Create_Callback()
		{
			var service = new CacheEntryOptionsFactory();
			var result = service.Create(new CacheOptions<string>
			{
				DidRemove = (x) => { }
			});
			Assert.Equal(1, result.PostEvictionCallbacks.Count);
		}
	}
}
