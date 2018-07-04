using System;
using System.Linq;
using System.Collections.Generic;

using ServiceStack.Redis;

using Moq;
using Xunit;

namespace CmsZwo.Cache.Tests
{
	public class RedisGatewayServiceTests
	{
		[Fact]
		public async void Set_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();
			var IRedisClient = new Mock<IRedisClient>();

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			await service.SetAsync("key", "value");

			IRedisClient.Verify(x => x.Set("key", "value".ToJsonWithTypeInformation()), Times.Once);
		}

		[Fact]
		public async void Set_Should_Set_Expiry()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();
			var IRedisClient = new Mock<IRedisClient>();

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			await service.SetAsync("key", "value", new CacheOptions<string>(CacheExpirationType.Absolute, timeoutMinutes: 10));

			IRedisClient.Verify(x => x.ExpireEntryAt("key", It.IsAny<DateTime>()), Times.Once);
		}

		[Fact]
		public async void Get_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var IRedisClient = new Mock<IRedisClient>();
			IRedisClient.Setup(x => x.Get<string>("key")).Returns("value".ToJsonWithTypeInformation());

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			var result = await service.GetAsync<string>("key");
			Assert.Equal("value", result);
		}

		[Fact]
		public async void GetMany_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var dict = new Dictionary<string, string>
			{
				{ "key1", "value1" },
				{ "key2", "value2" }
			};

			var IRedisClient = new Mock<IRedisClient>();
			IRedisClient
				.Setup(x => x.GetAll<string>(dict.Keys))
				.Returns(dict.ToDictionary(x => x.Key, x => x.Value.ToJsonWithTypeInformation()));

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			var result = await service.GetManyAsync<string>(dict.Keys);
			foreach (var value in result)
				Assert.True(dict.Values.ContainsIgnoreCase(value));
		}

		[Fact]
		public async void SetSet_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var IRedisClient = new Mock<IRedisClient>();

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			var values = new[] { "v1", "v2" };
			await service.SetSetAsync("key", values);

			IRedisClient.Verify(x => x.AddRangeToSet("key", It.IsAny<List<string>>()), Times.Once);
		}

		[Fact]
		public async void GetSet_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var values = new HashSet<string> { "v1", "v2" };

			var IRedisClient = new Mock<IRedisClient>();
			IRedisClient
				.Setup(x => x.GetAllItemsFromSet("key"))
				.Returns(values);

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			var result = await service.GetSetAsync<string>("key");
			foreach (var value in result)
				Assert.True(values.ContainsIgnoreCase(value));
		}

		[Fact]
		public async void AddToSet_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var values = new HashSet<string> { "v1", "v2" };

			var IRedisClient = new Mock<IRedisClient>();
			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			await service.AddToSetAsync("key", values);

			IRedisClient.Verify(x => x.AddRangeToSet("key", It.IsAny<List<string>>()), Times.Once);
		}

		[Fact]
		public async void RemoveFromSet_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var IRedisClient = new Mock<IRedisClient>();
			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			await service.RemoveFromSetAsync("key", "value");

			IRedisClient.Verify(x => x.RemoveItemFromSet("key", "value"), Times.Once);
		}

		[Fact]
		public async void Remove_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var IRedisClient = new Mock<IRedisClient>();
			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			await service.RemoveAsync("key");

			IRedisClient.Verify(x => x.Remove("key"), Times.Once);
		}

		[Fact]
		public async void Remove_Many_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var keys = new[] { "key1", "key2" };

			var IRedisClient = new Mock<IRedisClient>();
			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			await service.RemoveAsync(keys);

			IRedisClient.Verify(x => x.RemoveAll(keys), Times.Once);
		}

		[Fact]
		public async void IsCached_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var IRedisClient = new Mock<IRedisClient>();
			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			await service.IsCachedAsync("key");

			IRedisClient.Verify(x => x.ContainsKey("key"), Times.Once);
		}

		[Fact]
		public async void Clear_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var IRedisClient = new Mock<IRedisClient>();
			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			await service.ClearAsync();

			IRedisClient.Verify(x => x.FlushDb(), Times.Once);
		}

		[Fact]
		public async void ClearIfRequired_With_Higher_Version_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var versionInRedis = 10;
			var versionInClient = 9;

			var ISettingsService = Mock.Get(service.ISettingsService);
			ISettingsService
				.Setup(x => x.Get<CacheServiceSettings>())
				.Returns(new CacheServiceSettings { Redis = new CacheServiceSettings.RedisSettings { DataVersion = versionInRedis } });

			var IRedisClient = new Mock<IRedisClient>();
			IRedisClient.Setup(x => x.Get<int>(It.IsAny<string>())).Returns(versionInClient);

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			await service.ClearIfRequiredAsync();

			IRedisClient.Verify(x => x.FlushDb(), Times.Once);
			IRedisClient.Verify(x => x.Set(It.IsAny<string>(), versionInRedis), Times.Once);
		}

		[Fact]
		public async void ClearIfRequired_With_Lower_Version_Should_Not_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var versionInRedis = 9;
			var versionInClient = 10;

			var ISettingsService = Mock.Get(service.ISettingsService);
			ISettingsService
				.Setup(x => x.Get<CacheServiceSettings>())
				.Returns(new CacheServiceSettings { Redis = new CacheServiceSettings.RedisSettings { DataVersion = versionInRedis } });

			var IRedisClient = new Mock<IRedisClient>();
			IRedisClient.Setup(x => x.Get<int>(It.IsAny<string>())).Returns(versionInClient);

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			await service.ClearIfRequiredAsync();

			IRedisClient.Verify(x => x.FlushDb(), Times.Never);
			IRedisClient.Verify(x => x.Set(It.IsAny<string>(), versionInRedis), Times.Never);
		}

		[Fact]
		public async void IncrementInHash_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var current = 10;
			var steps = 2;

			var IRedisClient = new Mock<IRedisClient>();
			IRedisClient.Setup(x => x.IncrementValueInHash("hash", "key", steps)).Returns(current + steps);

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			var result = await service.IncrementInHashAsync("hash", "key", current, steps);
			Assert.Equal(current + steps, result);
		}

		[Fact]
		public async void IncrementInHash_Should_Recover()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var redis = 1;
			var current = 10;
			var steps = 2;

			var IRedisClient = new Mock<IRedisClient>();
			IRedisClient.Setup(x => x.IncrementValueInHash("hash", "key", steps)).Returns(redis);

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			var result = await service.IncrementInHashAsync("hash", "key", current, steps);
			Assert.Equal(current + steps, result);

			IRedisClient.Verify(x => x.SetEntryInHash("hash", "key", result.ToString()), Times.Once);
		}

		[Fact]
		public async void RemoveFromHash_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var IRedisClient = new Mock<IRedisClient>();
			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			await service.RemoveFromHashAsync("hash", "key");

			IRedisClient.Verify(x => x.RemoveEntryFromHash("hash", "key"), Times.Once);
		}

		[Fact]
		public async void GetHashCounters_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var dict = new Dictionary<string, string> { { "key1", "1" }, { "key2", "2" } };

			var IRedisClient = new Mock<IRedisClient>();
			IRedisClient.Setup(x => x.GetAllEntriesFromHash("hash")).Returns(dict);

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			var result = await service.GetHashCountersAsync("hash");
			foreach (var p in result)
				Assert.Equal(dict[p.Key], p.Value.ToString());

			IRedisClient.Verify(x => x.GetAllEntriesFromHash("hash"), Times.Once);
		}

		[Fact]
		public async void IncrementCounter_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var current = 10;
			var steps = 2;

			var IRedisClient = new Mock<IRedisClient>();
			IRedisClient.Setup(x => x.Increment("key", (uint)steps)).Returns(current + steps);

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			var result = await service.IncrementCounterAsync("key", current, steps);
			Assert.Equal(current + steps, result);
		}

		[Fact]
		public async void IncrementCounter_Should_Recover()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var redis = 1;
			var current = 10;
			var steps = 2;

			var IRedisClient = new Mock<IRedisClient>();
			IRedisClient.Setup(x => x.Increment("key", (uint)steps)).Returns(redis);

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			var result = await service.IncrementCounterAsync("key", current, steps);
			Assert.Equal(current + steps, result);

			IRedisClient.Verify(x => x.Set("key", result), Times.Once);
		}

		[Fact]
		public async void ResetCounter_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var IRedisClient = new Mock<IRedisClient>();
			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			await service.ResetCounterAsync("key");

			IRedisClient.Verify(x => x.Remove("key"), Times.Once);
		}

		[Fact]
		public async void SetExpiration_Should_Invoke_Client()
		{
			var service = MoqHelper.CreateWithMocks<RedisGatewayService>();

			var IRedisClient = new Mock<IRedisClient>();
			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory.Setup(x => x.GetClient()).Returns(IRedisClient.Object);

			var ts = TimeSpan.FromMinutes(5);

			await service.SetExpirationAsync("key", ts);

			IRedisClient.Verify(x => x.ExpireEntryIn("key", ts), Times.Once);
		}
	}
}
