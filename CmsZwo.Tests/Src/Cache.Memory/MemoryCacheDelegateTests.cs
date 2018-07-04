using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Moq;
using Xunit;

using CmsZwo.Cache.Memory;

namespace CmsZwo.Cache.Tests
{
	public class MemoryCacheDelegateTests
	{
		private MemoryCacheDelegate CreateService()
		{
			var result = MoqHelper.CreateWithMocks<MemoryCacheDelegate>();
			result.IMemoryCacheService = new MemoryCacheService();
			return result;
		}

		[Fact]
		public async void Set_Get_Should_Return_Cache_Value()
		{
			var service = CreateService();

			var key = "key";
			var value = "Hello, World!";

			await service.SetAsync(key, value);
			var result = await service.GetAsync<string>(key);

			Assert.Equal(value, result);
		}

		[Fact]
		public async void Get_Should_Put_Into_Cache()
		{
			var service = CreateService();

			var key = "key";
			var value = "Hello, World!";

			var result = await service.GetAsync(key, () => value);
			Assert.Equal(value, result);

			Assert.True(await service.IsCachedAsync(key));
		}

		[Fact]
		public void Get_Create_Should_Not_Allow_Concurrent_Factory_Execution()
		{
			var service = CreateService();

			var concurrentWorker = 0;
			var maxWorkers = 0;

			var tasks = new List<Task>();

			for (var i = 0; i < 100; i++)
			{
				var task =
					Task.Run(() =>
					{
						var _x =
							service.GetAsync(
								"key",
								() =>
								{
									Console.WriteLine($"Starting worker {i}...");
									concurrentWorker++;

									if (concurrentWorker > maxWorkers)
										maxWorkers = concurrentWorker;

									Thread.Sleep(200);
									concurrentWorker--;

									Console.WriteLine($"Ending worker {i}...");

									return "Value";
								}
							);
					});

				tasks.Add(task);
			}

			Task.WaitAll(tasks.ToArray());

			Assert.Equal(1, maxWorkers);
		}

		[Fact]
		public async void IncrementCounterAsync_Sould_Increment_Counter_Value()
		{
			var service = CreateService();

			var delta = 4ul;

			var x1 = await service.IncrementCounterAsync(nameof(service), () => delta);
			Assert.Equal(delta + 1, x1);

			var x2 = await service.IncrementCounterAsync(nameof(service), () => delta, delta);
			Assert.Equal(x1 + delta, x2);
		}

		[Fact]
		public async void ResetCounterAsync_Should_Set_Counter_To_0()
		{
			var service = CreateService();

			var x1 = await service.IncrementCounterAsync(nameof(service), () => 0ul);
			Assert.Equal(1ul, x1);

			await service.ResetCounterAsync(nameof(service));
			var x2 = await service.IncrementCounterAsync(nameof(service), () => 0ul);
			Assert.Equal(1ul, x2);
		}

		[Fact]
		public async void GetManyAsync_Should_Return_All_Requested_Values()
		{
			var service = CreateService();

			var max = 10;

			for (var i = 0; i < max; i++)
				await service.SetAsync($"key{i}", i);

			var keys = new List<string>();
			for (var i = 0; i < max; i++)
				keys.Add($"key{i}");

			var values = await service.GetManyAsync<int>(keys);

			for (var i = 0; i < max; i++)
				Assert.Contains(i, values);
		}

		[Fact]
		public async void RemoveAsync_Should_Remove_Entry()
		{
			var service = CreateService();

			await service.SetAsync("key", "value");
			Assert.True(await service.IsCachedAsync("key"));

			await service.RemoveAsync("key");
			Assert.False(await service.IsCachedAsync("key"));
		}

		[Fact]
		public async void RemoveAsync_Many_Should_Remove_Entries()
		{
			var service = CreateService();

			var max = 10;

			for (var i = 0; i < max; i++)
			{
				await service.SetAsync($"key{i}", i);
				Assert.True(await service.IsCachedAsync($"key{i}"));
			}

			var keys = new List<string>();
			for (var i = 0; i < max; i++)
				keys.Add($"key{i}");

			await service.RemoveAsync(keys);

			for (var i = 0; i < max; i++)
				Assert.False(await service.IsCachedAsync($"key{i}"));
		}

		[Fact]
		public async void IsCachedAsync_Should_Return_True_Is_Set()
		{
			var service = CreateService();

			Assert.False(await service.IsCachedAsync("key"));
			await service.SetAsync("key", "value");
			Assert.True(await service.IsCachedAsync("key"));
		}

		[Fact]
		public async void GetSetAsync_Should_Return_Set_With_Values()
		{
			var service = CreateService();

			var max = 10;
			var keys = new List<string>();
			for (var i = 0; i < max; i++)
				keys.Add($"key{i}");

			var set = await service.GetSetAsync("key", () => keys);
			for (var i = 0; i < max; i++)
				Assert.Contains($"key{i}", set);
		}

		[Fact]
		public async void AddToSetAsync_Should_Add_Value_To_Set()
		{
			var service = CreateService();

			await service.GetSetAsync("key");
			await service.AddToSetAsync("key", "hello");

			var set = await service.GetSetAsync("key");
			Assert.Contains("hello", set);
		}

		[Fact]
		public async void AddToSetAsync_Should_Set_NotRemoveable_Hashset()
		{
			var service = CreateService();
			var IMemoryCacheService = new Mock<IMemoryCacheService>();
			service.IMemoryCacheService = IMemoryCacheService.Object;

			await service.GetSetAsync("key");

			IMemoryCacheService
				.Verify(
					x =>
						x.Set(
							It.IsAny<string>(),
							It.IsAny<HashSet<string>>(),
							It.Is<CacheOptions<HashSet<string>>>(o => o.ExpirationType == CacheExpirationType.NotRemoveable)
						),
					Times.Once()
				);
		}

		[Fact]
		public async void AddToSetAsync_Many_Should_Add_Values_To_Set()
		{
			var service = CreateService();

			await service.GetSetAsync("key");
			await service.AddToSetAsync("key", "hello");

			var set = await service.GetSetAsync("key");
			Assert.Contains("hello", set);
		}

		[Fact]
		public async void RemoveFromSetAsync_Should_Remove_Value_From_Set()
		{
			var service = CreateService();

			var max = 10;
			var keys = new List<string>();
			for (var i = 0; i < max; i++)
				keys.Add($"key{i}");

			var set = await service.GetSetAsync("key", () => keys);
			for (var i = 0; i < max; i++)
				await service.RemoveFromSetAsync("key", $"key{i}");

			set = await service.GetSetAsync("key", () => keys);
			Assert.False(set.HasContent());
		}

		[Fact]
		public async void RemoveFromSetAsync_Many_Should_Remove_Values_From_Set()
		{
			var service = CreateService();

			var max = 10;
			var keys = new List<string>();
			for (var i = 0; i < max; i++)
				keys.Add($"key{i}");

			var set = await service.GetSetAsync("key", () => keys);
			await service.RemoveFromSetAsync("key", keys);

			set = await service.GetSetAsync("key", () => keys);
			Assert.False(set.HasContent());
		}
	}
}
