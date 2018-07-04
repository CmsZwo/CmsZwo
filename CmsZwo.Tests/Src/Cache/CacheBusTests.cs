using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Moq;
using Xunit;

namespace CmsZwo.Cache.Tests
{
	public class CacheBusTests
	{
		[Fact]
		public void Start_Should_Create_Random_Instance_Id()
		{
			var service = MoqHelper.CreateWithMocks<TestCacheBus>();
			var IRandomService = Mock.Get(service.IRandomService);

			service.Start();
			IRandomService
				.Verify(x => x.RandomBase24(It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
		}

		[Fact]
		public async void Command_Without_Start_Should_Throw_Exception()
		{
			await Assert.ThrowsAsync<Exception>(async () =>
			{
				var service = MoqHelper.CreateWithMocks<TestCacheBus>();
				await service.RemoveAsync("1");
			});
		}

		[Fact]
		public async void RemoveAsync_Should_Add_Command()
		{
			var service = MoqHelper.CreateWithMocks<TestCacheBus>();
			service.Start();
			await service.RemoveAsync("1");
			Assert.Contains("RemoveAsync", service.Commands);
		}

		[Fact]
		public async void RemoveManyAsync_Should_Add_One_Command()
		{

			var service = MoqHelper.CreateWithMocks<TestCacheBus>();

			var count = 10;
			var list = new List<string>();
			for (var i = 0; i < count; i++)
				list.Add(i.ToString());

			service.Start();
			await service.RemoveManyAsync(list);
			Assert.Single(service.Commands);
			Assert.Contains("RemoveManyAsync", service.Commands);
		}

		[Fact]
		public async void AddToSetAsyncc_Should_Add_Command()
		{
			var service = MoqHelper.CreateWithMocks<TestCacheBus>();
			service.Start();
			await service.AddToSetAsync("key", new[] { "value " });
			Assert.Contains("AddToSetAsync", service.Commands);
		}

		[Fact]
		public async void RemoveFromSetAsyncc_Should_Add_Command()
		{
			var service = MoqHelper.CreateWithMocks<TestCacheBus>();
			service.Start();
			await service.RemoveFromSetAsync("key", "value");
			Assert.Contains("RemoveFromSetAsync", service.Commands);
		}

		[Fact]
		public async void ClearAsyncc_Should_Add_Command()
		{
			var service = MoqHelper.CreateWithMocks<TestCacheBus>();
			service.Start();
			await service.ClearAsync();
			Assert.Contains("ClearAsync", service.Commands);
		}

		[Fact]
		public void Same_InstanceId_Should_Be_Ignored()
		{
			var service = MoqHelper.CreateWithMocks<TestCacheBus>();

			var instanceId = "test";

			var IRandomService = Mock.Get(service.IRandomService);
			IRandomService
				.Setup(x => x.RandomBase24(It.IsAny<int>(), It.IsAny<bool>()))
				.Returns(instanceId);

			service.Start();
			service.ReceiveCommand(new CacheBusCommand
			{
				Sender = instanceId
			});

			var IMemoryCacheDelegate = Mock.Get(service.IMemoryCacheDelegate);
			IMemoryCacheDelegate.VerifyNoOtherCalls();
		}

		[Fact]
		public void Different_Target_Should_Be_Ignored()
		{
			var service = MoqHelper.CreateWithMocks<TestCacheBus>();

			var target = "test";

			service.Start();
			service.ReceiveCommand(new CacheBusCommand
			{
				Target = target
			});

			var IMemoryCacheDelegate = Mock.Get(service.IMemoryCacheDelegate);
			IMemoryCacheDelegate.VerifyNoOtherCalls();
		}

		[Fact]
		public void First_Transfer_Should_Only_Purge_After_Timeout()
		{
			var service = MoqHelper.CreateWithMocks<TestCacheBus>();

			var now = DateTime.Now;
			var ITimeService = Mock.Get(service.ITimeService);
			ITimeService.Setup(x => x.Now).Returns(now);

			service.Start();
			service.ClearAsync();

			Assert.Equal(0, service.Purged);

			ITimeService.Setup(x => x.Now).Returns(now.AddMinutes(30));
			service.ClearAsync();

			Assert.Equal(1, service.Purged);
		}

		[Fact]
		public void First_Transfer_Should_Adopt_Sequence()
		{
			var service = MoqHelper.CreateWithMocks<TestCacheBus>();

			var now = DateTime.Now;
			var ITimeService = Mock.Get(service.ITimeService);
			ITimeService.Setup(x => x.Now).Returns(now);

			service.Start();
			service.ClearAsync();

			ITimeService.Setup(x => x.Now).Returns(now.AddMinutes(30));
			service.ClearAsync();

			Assert.Equal(101, service.NextPurgeSequence);
		}

		[Fact]
		public void Lower_Sequence_Should_Be_Ignored()
		{
			var service = MoqHelper.CreateWithMocks<TestCacheBus>();
			var IMemoryCacheDelegate = Mock.Get(service.IMemoryCacheDelegate);

			service.ReceiveCommand(new CacheBusCommand
			{
				Sequence = 100,
				Sender = "other",
				Name = nameof(service.IMemoryCacheDelegate.ClearAsync)
			});

			IMemoryCacheDelegate.Verify(x => x.ClearAsync(), Times.Once);
			IMemoryCacheDelegate.ResetCalls();

			service.ReceiveCommand(new CacheBusCommand
			{
				Sequence = 80,
				Sender = "other",
				Name = nameof(service.IMemoryCacheDelegate.ClearAsync)
			});

			IMemoryCacheDelegate.Verify(x => x.ClearAsync(), Times.Never);
		}

		[Fact]
		public void Higher_Sequence_Should_Invoke_PullMissingMessages()
		{
			var service = MoqHelper.CreateWithMocks<TestCacheBus>();
			var IMemoryCacheDelegate = Mock.Get(service.IMemoryCacheDelegate);

			service.ReceiveCommand(new CacheBusCommand
			{
				Sequence = 100,
				Sender = "other",
				Name = nameof(service.IMemoryCacheDelegate.ClearAsync)
			});

			Assert.Equal(0, service.PulledMissing);

			service.ReceiveCommand(new CacheBusCommand
			{
				Sequence = 200,
				Sender = "other",
				Name = nameof(service.IMemoryCacheDelegate.ClearAsync)
			});

			Assert.Equal(1, service.PulledMissing);
		}

		public class TestCacheBus : CacheBus
		{
			public List<string> Commands { get; }
				= new List<string>();

			public void ReceiveCommand(CacheBusCommand command)
				=> OnReciveCommand(command);

			private long _Sequence = 100;

			protected override Task TransferCommand(CacheBusCommand command)
			{
				Commands.Add(command.Name);
				command.Sequence = ++_Sequence;
				return Task.CompletedTask;
			}

			public int PulledMissing { get; set; }

			protected override void PullMissingMessages(long startingSequence)
			{
				PulledMissing++;
			}

			public int Purged { get; set; }
			public long NextPurgeSequence { get; set; }

			protected override Task Purge(long nextPurgeSequence)
			{
				Purged++;
				NextPurgeSequence = nextPurgeSequence;
				return Task.CompletedTask;
			}
		}
	}
}
