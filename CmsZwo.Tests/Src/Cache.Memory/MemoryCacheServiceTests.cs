using CmsZwo.Cache.Memory;
using Microsoft.Extensions.Caching.Memory;

using Moq;
using Xunit;

namespace CmsZwo.Cache.Tests
{
	public class MemoryCacheServiceTests
	{
		[Fact]
		public void GetOrCreate_Should_Create_If_Not_Set()
		{
			var cache = new MemoryCache(new MemoryCacheOptions());
			var service = MoqHelper.CreateWithMocks<MemoryCacheService>();
			var IMemoryCacheFactory = Mock.Get(service.IMemoryCacheFactory);
			IMemoryCacheFactory.Setup(x => x.Create()).Returns(cache);

			var key = "key";
			var value = "Hello, World!";

			service.GetOrCreate(key, x => value);

			var test = cache.Get(key);
			Assert.Equal(value, test);
		}

		[Fact]
		public void Set_Should_Set_Value()
		{
			var cache = new MemoryCache(new MemoryCacheOptions());
			var service = MoqHelper.CreateWithMocks<MemoryCacheService>();
			var IMemoryCacheFactory = Mock.Get(service.IMemoryCacheFactory);
			IMemoryCacheFactory.Setup(x => x.Create()).Returns(cache);

			var key = "key";
			var value = "Hello, World!";

			service.Set(key,value);

			var test = cache.Get(key);
			Assert.Equal(value, test);
		}

		[Fact]
		public void Get_Should_Return_Value_As_Object()
		{
			var cache = new MemoryCache(new MemoryCacheOptions());
			var service = MoqHelper.CreateWithMocks<MemoryCacheService>();
			var IMemoryCacheFactory = Mock.Get(service.IMemoryCacheFactory);
			IMemoryCacheFactory.Setup(x => x.Create()).Returns(cache);

			var key = "key";
			var value = "Hello, World!";

			cache.Set(key, value);
			var test = service.Get(key);

			Assert.Equal(value, test);
		}

		[Fact]
		public void Get_Should_Return_Value_As_T()
		{
			var cache = new MemoryCache(new MemoryCacheOptions());
			var service = MoqHelper.CreateWithMocks<MemoryCacheService>();
			var IMemoryCacheFactory = Mock.Get(service.IMemoryCacheFactory);
			IMemoryCacheFactory.Setup(x => x.Create()).Returns(cache);

			var key = "key";
			var value = "Hello, World!";

			cache.Set(key, value);
			var test = service.Get<string>(key);

			Assert.Equal(value, test);
		}

		[Fact]
		public void Remove_Should_Remove_Value()
		{
			var cache = new MemoryCache(new MemoryCacheOptions());
			var service = MoqHelper.CreateWithMocks<MemoryCacheService>();
			var IMemoryCacheFactory = Mock.Get(service.IMemoryCacheFactory);
			IMemoryCacheFactory.Setup(x => x.Create()).Returns(cache);

			var key = "key";
			var value = "Hello, World!";

			cache.Set(key, value);

			Assert.NotNull(cache.Get(key));
			service.Remove(key);
			Assert.Null(cache.Get(key));
		}

		[Fact]
		public void Clear_Should_Remove_All_Values()
		{
			var service = new MemoryCacheService();

			var key = "key";
			var value = "Hello, World!";

			service.Set(key, value);

			Assert.NotNull(service.Get(key));
			service.Clear();
			Assert.Null(service.Get(key));
		}
	}
}
