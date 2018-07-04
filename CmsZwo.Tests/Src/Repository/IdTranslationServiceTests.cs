using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Moq;
using Xunit;

using CmsZwo.Cache;

namespace CmsZwo.Repository.Tests
{
	public class IdTranslationServiceTests
	{
		public class DummyEntity : Entity { }

		[Fact]
		public async void With_Cache_Should_Return_Cache_Value()
		{
			var service = MoqHelper.CreateWithMocks<IdTranslationService>();
			var ICacheService = Mock.Get(service.ICacheService);

			ICacheService
				.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Func<string>>(), It.IsAny<CacheOptions<string>>()))
				.Returns(Task.FromResult("id"));

			var verify = await service.GetIdByNameAsync<DummyEntity>("Foo");
			Assert.Equal("id", verify);
		}

		[Fact]
		public async void GetIdByName_Should_QueryName()
		{
			var service = MoqHelper.CreateWithMocks<IdTranslationService>();
			var ICacheService = Mock.Get(service.ICacheService);
			var IDatabaseService = Mock.Get(service.IDatabaseService);

			var store = new List<DummyEntity>
			{
				new DummyEntity
				{
					Id = "id",
					Name = "Foo"
				}
			};

			ICacheService
				.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Func<string>>(), It.IsAny<CacheOptions<string>>()))
				.Returns<string, Func<string>, CacheOptions<string>>((key, factory, options) => Task.FromResult(factory()));

			IDatabaseService
				.Setup(x => x.GetQueryable<DummyEntity>())
				.Returns(store.AsQueryable());

			var verify = await service.GetIdByNameAsync<DummyEntity>("Foo");
			Assert.Equal("id", verify);
		}

		[Fact]
		public async void GetIdByAutoId_Should_AutoId()
		{
			var service = MoqHelper.CreateWithMocks<IdTranslationService>();
			var ICacheService = Mock.Get(service.ICacheService);
			var IDatabaseService = Mock.Get(service.IDatabaseService);

			var store = new List<DummyEntity>
			{
				new DummyEntity
				{
					Id = "id",
					AutoId = 123
				}
			};

			ICacheService
				.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<Func<string>>(), It.IsAny<CacheOptions<string>>()))
				.Returns<string, Func<string>, CacheOptions<string>>((key, factory, options) => Task.FromResult(factory()));

			IDatabaseService
				.Setup(x => x.GetQueryable<DummyEntity>())
				.Returns(store.AsQueryable());

			var verify = await service.GetIdByAutoIdAsync<DummyEntity>(123);
			Assert.Equal("id", verify);
		}

		[Fact]
		public async void PresetName_Should_Invoke_Cache()
		{
			var service = MoqHelper.CreateWithMocks<IdTranslationService>();
			var ICacheService = Mock.Get(service.ICacheService);

			await service.PresetNameAsync(new DummyEntity { Id = "id", Name = "Foo" });
			ICacheService.VerifyIgnoreArgs(x => x.SetAsync<string>(null, null, null), Times.Once);
		}
	}
}
