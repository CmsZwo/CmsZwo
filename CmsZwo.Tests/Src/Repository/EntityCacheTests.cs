using Moq;
using Xunit;

namespace CmsZwo.Repository.Tests
{
	public class EntityCacheTests
	{
		public class DummyEntity : Entity { }

		[Fact]
		public async void Set_Should_Invoke_Cache()
		{
			var service = MoqHelper.CreateWithMocks<EntityCacheService>();
			var ICacheService = Mock.Get(service.ICacheService);

			var dummy = new DummyEntity();
			await service.SetAsync(dummy);

			ICacheService.VerifyIgnoreArgs(x => x.SetAsync<DummyEntity>(null, null, null), Times.Once);
		}

		[Fact]
		public async void Remove_Should_Invoke_Cache()
		{
			var service = MoqHelper.CreateWithMocks<EntityCacheService>();
			var ICacheService = Mock.Get(service.ICacheService);

			var dummy = new DummyEntity();
			await service.RemoveAsync<DummyEntity>("123");

			ICacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public async void GetById_Should_Invoke_Cache()
		{
			var service = MoqHelper.CreateWithMocks<EntityCacheService>();
			var ICacheService = Mock.Get(service.ICacheService);

			await service.GetByIdAsync<DummyEntity>("123");

			ICacheService.VerifyIgnoreArgs(x => x.GetAsync<DummyEntity>(null, null, null), Times.Once);
		}
	}
}
