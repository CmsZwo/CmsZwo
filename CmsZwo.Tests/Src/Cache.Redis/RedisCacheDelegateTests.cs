using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Moq;
using Xunit;

using CmsZwo.Cache.Redis;

namespace CmsZwo.Cache.Tests
{
	public class RedisCacheDelegateTests
	{
		private RedisCacheDelegate CreateService()
			=> MoqHelper.CreateWithMocks<RedisCacheDelegate>();

		[Fact]
		public async void ResetCounter_Should_Invoke_Redis_Cache_And_Bus()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IRedisCacheBus = Mock.Get(service.IRedisCacheBus);

			await service.ResetCounterAsync("key");

			IRedisGatewayService.Verify(
				x => x.ResetCounterAsync(It.IsAny<string>()),
				Times.Once
			);

			IRedisCacheBus.Verify(
				x => x.RemoveAsync(It.IsAny<string>()),
				Times.Once
			);
		}

		[Fact]
		public async void Get_With_Value_Should_Get_Redis_Then_Set_Memory()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IMemoryCacheService = Mock.Get(service.IMemoryCacheService);

			IRedisGatewayService
				.Setup(x => x.GetAsync<string>(It.IsAny<string>()))
				.Returns(Task.FromResult("value"));

			var result = await service.GetAsync<string>("key");
			Assert.Equal("value", result);

			IMemoryCacheService.Verify(
				x => x.Get<string>(It.IsAny<string>()),
				Times.Once
			);

			IRedisGatewayService.Verify(
				x => x.GetAsync<string>(It.IsAny<string>()),
				Times.Once
			);

			IMemoryCacheService.Verify(
				x => x.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CacheOptions<string>>()),
				Times.Once
			);
		}

		[Fact]
		public async void Get_Without_Value_Should_Get_Redis_Then_Set_Memory_Then_Set_Redis()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IMemoryCacheService = Mock.Get(service.IMemoryCacheService);

			var result = await service.GetAsync("key", () => "value");
			Assert.Equal("value", result);

			IMemoryCacheService.Verify(
				x => x.Get<string>(It.IsAny<string>()),
				Times.Once
			);

			IRedisGatewayService.Verify(
				x => x.GetAsync<string>(It.IsAny<string>()),
				Times.Once
			);

			IMemoryCacheService.Verify(
				x => x.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CacheOptions<string>>()),
				Times.Once
			);

			IRedisGatewayService.Verify(
				x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CacheOptions<string>>()),
				Times.Once
			);
		}

		[Fact]
		public async void Get_Without_Value_And_No_Factory_Should_Set_NotFound()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IMemoryCacheService = Mock.Get(service.IMemoryCacheService);

			var result = await service.GetAsync<string>("key");
			Assert.Null(result);

			IMemoryCacheService.Verify(
				x => x.Get<string>(It.IsAny<string>()),
				Times.Once
			);

			IRedisGatewayService.Verify(
				x => x.GetAsync<string>(It.IsAny<string>()),
				Times.Once
			);

			IMemoryCacheService.Verify(
				x => x.Set<NotFound>(It.IsAny<string>(), It.IsAny<NotFound>(), It.IsAny<CacheOptions<NotFound>>()),
				Times.Once
			);
		}

		[Fact]
		public async void GetMany_Should_Invoke_Redis()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);

			var keys = new[] { "key1", "key2" };
			await service.GetManyAsync<string>(keys);

			IRedisGatewayService.Verify(
				x => x.GetManyAsync<string>(keys),
				Times.Once
			);
		}

		[Fact]
		public async void SetAsync_With_New_Value_Should_Invoke_Redis_And_RedisBus()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IRedisCacheBus = Mock.Get(service.IRedisCacheBus);

			IRedisGatewayService
				.Setup(x => x.GetAsync<string>(It.IsAny<string>()))
				.Returns(Task.FromResult("value"));

			await service.SetAsync("key", "new-value");

			IRedisGatewayService.Verify(
				x => x.GetAsync<string>(It.IsAny<string>()),
				Times.Once
			);

			IRedisGatewayService.Verify(
				x => x.SetAsync<string>(It.IsAny<string>(), "new-value", It.IsAny<CacheOptions<string>>()),
				Times.Once
			);

			IRedisCacheBus.Verify(
				x => x.RemoveAsync(It.IsAny<string>()),
				Times.Once
			);
		}

		[Fact]
		public async void SetAsync_With_Same_Value_Should_Not_Invoke_RedisBus()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IRedisCacheBus = Mock.Get(service.IRedisCacheBus);

			IRedisGatewayService
				.Setup(x => x.GetAsync<string>(It.IsAny<string>()))
				.Returns(Task.FromResult("value"));

			await service.SetAsync("key", "value");

			IRedisCacheBus.Verify(
				x => x.RemoveAsync(It.IsAny<string>()),
				Times.Never
			);
		}

		[Fact]
		public async void Remove_With_Empty_Keys_Should_Not_Invoke_Redis()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IRedisCacheBus = Mock.Get(service.IRedisCacheBus);

			await service.RemoveAsync(new string[] { });

			IRedisGatewayService.Verify(
				x => x.RemoveAsync(It.IsAny<string>()),
				Times.Never
			);

			IRedisCacheBus.Verify(
				x => x.RemoveAsync(It.IsAny<string>()),
				Times.Never
			);
		}

		[Fact]
		public async void Remove_Should_Invoke_Redis()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IRedisCacheBus = Mock.Get(service.IRedisCacheBus);

			var keys = new[] { "key1", "key2" };
			await service.RemoveAsync(keys);

			IRedisGatewayService.Verify(
				x => x.RemoveAsync(keys),
				Times.Once
			);

			IRedisCacheBus.Verify(
				x => x.RemoveManyAsync(keys),
				Times.Once
			);
		}

		[Fact]
		public async void IsCached_Should_Read_Local_Then_Redis()
		{
			var service = CreateService();
			var IMemoryCacheService = Mock.Get(service.IMemoryCacheService);
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);

			IRedisGatewayService
				.Setup(x => x.IsCachedAsync("key"))
				.Returns(Task.FromResult(true));

			var result = await service.IsCachedAsync("key");
			Assert.True(result);

			IMemoryCacheService.Verify(
				x => x.Get("key"),
				Times.Once
			);

			IRedisGatewayService.Verify(
				x => x.IsCachedAsync("key"),
				Times.Once
			);
		}

		[Fact]
		public async void IsCached_Should_Not_Invoke_Redis_If_Local_Has_Value()
		{
			var service = CreateService();
			var IMemoryCacheService = Mock.Get(service.IMemoryCacheService);
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);

			IMemoryCacheService
				.Setup(x => x.Get("key"))
				.Returns("value");

			var result = await service.IsCachedAsync("key");
			Assert.True(result);

			IMemoryCacheService.Verify(
				x => x.Get("key"),
				Times.Once
			);

			IRedisGatewayService.Verify(
				x => x.IsCachedAsync("key"),
				Times.Never
			);
		}

		[Fact]
		public async void IsCached_Should_Return_False_If_No_Value()
		{
			var service = CreateService();
			var result = await service.IsCachedAsync("key");
			Assert.False(result);
		}

		[Fact]
		public async void Clear_Should_Invoke_Redis_And_Bus()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IRedisCacheBus = Mock.Get(service.IRedisCacheBus);

			await service.ClearAsync();

			IRedisGatewayService.Verify(
				x => x.ClearAsync(),
				Times.Once
			);

			IRedisCacheBus.Verify(
				x => x.ClearAsync(),
				Times.Once
			);
		}

		[Fact]
		public async void GetSet_Should_Invoke_Local_Then_Redis()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IMemoryCacheService = Mock.Get(service.IMemoryCacheService);

			var hashSet = new HashSet<string>() { "value" };

			IRedisGatewayService
				.Setup(x => x.GetSetAsync<string>("key"))
				.Returns(Task.FromResult(hashSet));

			var result = await service.GetSetAsync("key");
			Assert.Equal(hashSet, result);

			IMemoryCacheService.Verify(
				x => x.Get<HashSet<string>>("key"),
				Times.Once
			);

			IRedisGatewayService.Verify(
				x => x.GetSetAsync<string>("key"),
				Times.Once
			);
		}

		[Fact]
		public async void GetSet_Factory_Invokation_Should_Set_Local_With_Not_Removeable()
		{
			var service = CreateService();
			var IMemoryCacheService = Mock.Get(service.IMemoryCacheService);

			var hashSet = new HashSet<string>() { "value" };

			var result = await service.GetSetAsync("key", () => hashSet);
			Assert.Equal(hashSet, result);

			IMemoryCacheService.Verify(
				x => x.Set<HashSet<string>>("key", hashSet, It.Is<CacheOptions<HashSet<string>>>(y => y.ExpirationType == CacheExpirationType.NotRemoveable)),
				Times.Once
			);
		}

		[Fact]
		public async void GetSet_With_Local_Value_Should_Not_Invoke_Redis()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IMemoryCacheService = Mock.Get(service.IMemoryCacheService);

			var hashSet = new HashSet<string>() { "value" };

			IMemoryCacheService
				.Setup(x => x.Get<HashSet<string>>("key"))
				.Returns(hashSet);

			var result = await service.GetSetAsync("key");
			Assert.Equal(hashSet, result);

			IRedisGatewayService.Verify(
				x => x.GetSetAsync<string>("key"),
				Times.Never
			);
		}

		[Fact]
		public async void AddToSet_Should_Invoke_Redis_And_Bus()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IRedisCacheBus = Mock.Get(service.IRedisCacheBus);

			var key = "key";
			var items = new[] { "item1", "item2" };
			await service.AddToSetAsync(key, items);

			IRedisGatewayService.Verify(
				x => x.AddToSetAsync(key, items),
				Times.Once
			);

			IRedisCacheBus.Verify(
				x => x.AddToSetAsync(key, items),
				Times.Once
			);
		}

		[Fact]
		public async void RemoveFromSet_Should_Invoke_Redis_And_Bus()
		{
			var service = CreateService();
			var IRedisGatewayService = Mock.Get(service.IRedisGatewayService);
			var IRedisCacheBus = Mock.Get(service.IRedisCacheBus);

			var key = "key";
			var items = new[] { "item1", "item2" };
			await service.RemoveFromSetAsync(key, items);

			IRedisGatewayService.Verify(
				x => x.RemoveFromSetAsync(key, It.IsAny<string>()),
				Times.Exactly(items.Count())
			);

			IRedisCacheBus.Verify(
				x => x.RemoveFromSetAsync(key, It.IsAny<string>()),
				Times.Exactly(items.Count())
			);
		}
	}
}
