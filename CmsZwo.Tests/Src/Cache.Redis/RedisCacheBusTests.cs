using System;
using System.Threading;
using System.Collections.Generic;

using ServiceStack.Redis;

using CmsZwo.Cache.Redis;

using Moq;
using Xunit;

namespace CmsZwo.Cache.Tests
{
	public class RedisCacheBusTests
	{
		private RedisCacheBus CreateService()
		{
			var service = MoqHelper.CreateWithMocks<RedisCacheBus>();

			var IThreadService = Mock.Get(service.IThreadService);
			IThreadService
				.Setup(x => x.QueueUserWorkItem(It.IsAny<WaitCallback>()))
				.Callback<WaitCallback>(x => x.Invoke(null));

			var ISettingsService = Mock.Get(service.ISettingsService);
			ISettingsService
				.Setup(x => x.Get<CacheServiceSettings>())
				.Returns(new CacheServiceSettings());

			var IRedisSubscription = new Mock<IRedisSubscription>();

			var IRedisClient = new Mock<IRedisClient>();
			IRedisClient
				.Setup(x => x.CreateSubscription())
				.Returns(IRedisSubscription.Object);

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory
				.Setup(x => x.GetClient())
				.Returns(IRedisClient.Object);

			return service;
		}

		private Action<string, string> StartAndCaptureOnMessage(RedisCacheBus service)
		{
			var IRedisSubscription = new Mock<IRedisSubscription>();

			var IRedisClient = Mock.Get(service.IRedisClientFactory.GetClient());
			IRedisClient
				.Setup(x => x.CreateSubscription())
				.Returns(IRedisSubscription.Object);

			Action<string, string> result = null;

			IRedisSubscription
				.SetupSet<Action<string, string>>(x => x.OnMessage = It.IsAny<Action<string, string>>())
				.Callback(x => result = x);

			service.Start();

			return result;
		}

		[Fact]
		public void Start_Should_Subscribe_To_Channel()
		{
			var service = CreateService();
			var IRedisClient = Mock.Get(service.IRedisClientFactory.GetClient());
			var IRedisSubscription = Mock.Get(IRedisClient.Object.CreateSubscription());

			service.Start();

			IRedisSubscription.Verify(x => x.SubscribeToChannels(It.IsAny<string[]>()), Times.Once);
		}

		[Fact]
		public void Start_Should_Recover_From_Redis_Exception()
		{
			var service = CreateService();

			var fistCall = true;

			var IRedisClient = Mock.Get(service.IRedisClientFactory.GetClient());

			var IRedisClientFactory = Mock.Get(service.IRedisClientFactory);
			IRedisClientFactory
				.Setup(x => x.GetClient())
				.Callback(() =>
				{
					if (!fistCall)
						return;

					fistCall = false;
					throw new RedisException("test");
				})
				.Returns(IRedisClient.Object);

			service.Start();

			var IThreadService = Mock.Get(service.IThreadService);
			IThreadService.Verify(x => x.Sleep(It.IsAny<TimeSpan>()), Times.Once);
		}

		[Fact]
		public async void Send_Command_Should_Transfer_Command()
		{
			var service = CreateService();

			var IRedisClient = Mock.Get(service.IRedisClientFactory.GetClient());

			service.Start();
			await service.ClearAsync();

			IRedisClient.Verify(x => x.Increment(It.IsAny<string>(), 1), Times.Once);
		}

		[Fact]
		public async void Send_Command_Should_Add_Message_To_QueueSet()
		{
			var service = CreateService();

			var IRedisClient = Mock.Get(service.IRedisClientFactory.GetClient());

			service.Start();
			await service.ClearAsync();

			IRedisClient.Verify(x => x.AddItemToSortedSet(It.IsAny<string>(), It.IsAny<string>(), 0), Times.Once);
			IRedisClient.Verify(x => x.PublishMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public void PullMissingMessages_Should_Receive_Missing_Commands()
		{
			var service = CreateService();

			var missingCommands = new Dictionary<string, double>
			{
				{
					new CacheBusCommand{
						Sender = "other",
						Target = "_Sync",
						Name = nameof(service.IMemoryCacheDelegate.ClearAsync)
					}.ToJsonWithTypeInformation()
				, 101d}
			};

			var IRedisClient = Mock.Get(service.IRedisClientFactory.GetClient());
			IRedisClient.Setup(x => x.GetAllWithScoresFromSortedSet(It.IsAny<string>())).Returns(missingCommands);

			var onMessage = StartAndCaptureOnMessage(service);

			onMessage.Invoke("channel", new CacheBusCommand
			{
				Sender = "other",
				Sequence = 100
			}.ToJsonWithTypeInformation());

			onMessage.Invoke("channel", new CacheBusCommand
			{
				Sender = "other",
				Sequence = 200
			}.ToJsonWithTypeInformation());

			var IMemoryCacheDelegate = Mock.Get(service.IMemoryCacheDelegate);
			IMemoryCacheDelegate.Verify(x => x.ClearAsync(), Times.Once);
		}
	}
}
