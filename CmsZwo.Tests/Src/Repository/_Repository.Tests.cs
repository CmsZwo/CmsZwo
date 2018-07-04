using System;
using System.Threading.Tasks;

using Moq;
using Xunit;

namespace CmsZwo.Repository.Tests
{
	public class RepositoryTests
	{
		private class DummyEntity : Entity { }
		private class DummyRepository : Repository<DummyEntity> { }

		[Fact]
		public void Container_Should_Return_IRepository_Of_Type()
		{
			var container = new Container.Container();
			container.Register<DummyRepository>();

			var verify = container.Get<IRepository<DummyEntity>>();
			Assert.NotNull(verify);
			Assert.True(verify is IRepository<DummyEntity>);
		}

		[Fact]
		public void GetQueryable_Should_Invoke_DatabaseService()
		{
			var service = MoqHelper.CreateWithMocks<DummyRepository>();
			var IDatabaseService = Mock.Get(service.IDatabaseService);

			service.GetQueryable();
			IDatabaseService.Verify(x => x.GetQueryable<DummyEntity>(), Times.Once);
		}

		[Fact]
		public async void GetById_None_Should_Invoke_Database_And_Cache()
		{
			var service = MoqHelper.CreateWithMocks<DummyRepository>();
			var IDatabaseService = Mock.Get(service.IDatabaseService);
			var IEntityCacheService = Mock.Get(service.IEntityCacheService);

			IDatabaseService
				.Setup(x => x.GetByIdAsync<DummyEntity>(It.IsAny<string>()))
				.Returns(Task.FromResult(new DummyEntity()));

			await service.GetByIdAsync("id");
			IDatabaseService.Verify(x => x.GetByIdAsync<DummyEntity>(It.IsAny<string>()), Times.Once);
			IEntityCacheService.Verify(x => x.SetAsync(It.IsAny<DummyEntity>()), Times.Once);
		}

		[Fact]
		public async void GetById_Cached_Should_Not_Invoke_Database_And_Cache()
		{
			var service = MoqHelper.CreateWithMocks<DummyRepository>();
			var IDatabaseService = Mock.Get(service.IDatabaseService);
			var IEntityCacheService = Mock.Get(service.IEntityCacheService);

			IEntityCacheService
				.Setup(x => x.GetByIdAsync<DummyEntity>(It.IsAny<string>()))
				.Returns(Task.FromResult(new DummyEntity()));

			await service.GetByIdAsync("id");
			IDatabaseService.Verify(x => x.GetByIdAsync<DummyEntity>(It.IsAny<string>()), Times.Never);
			IEntityCacheService.Verify(x => x.SetAsync(It.IsAny<DummyEntity>()), Times.Never);
		}

		[Fact]
		public async void GetById_None_With_No_Database_Should_Not_Invoke_Cache()
		{
			var service = MoqHelper.CreateWithMocks<DummyRepository>();
			var IDatabaseService = Mock.Get(service.IDatabaseService);
			var IEntityCacheService = Mock.Get(service.IEntityCacheService);

			await service.GetByIdAsync("id");

			IEntityCacheService.Verify(x => x.SetAsync(It.IsAny<DummyEntity>()), Times.Never);
		}

		[Fact]
		public async void Insert_Should_Set_Created_And_AutoId()
		{
			var service = MoqHelper.CreateWithMocks<DummyRepository>();
			var ITimeService = Mock.Get(service.ITimeService);
			var IAutoIdService = Mock.Get(service.IAutoIdService);

			ITimeService.Setup(x => x.Now).Returns(DateTime.Now);
			IAutoIdService.Setup(x => x.GetNext<DummyEntity>()).Returns(Task.FromResult(1ul));

			var entity = new DummyEntity();
			await service.InsertAsync(entity);

			Assert.NotNull(entity.Created);
			Assert.Equal(1ul, entity.AutoId);
		}

		[Fact]
		public async void Insert_Should_Invoke_Database_And_Cache()
		{
			var service = MoqHelper.CreateWithMocks<DummyRepository>();
			var IDatabaseService = Mock.Get(service.IDatabaseService);
			var IEntityCacheService = Mock.Get(service.IEntityCacheService);

			var entity = new DummyEntity();
			await service.InsertAsync(entity);

			IDatabaseService.Verify(x => x.InsertAsync(It.IsAny<DummyEntity>()), Times.Once);
			IEntityCacheService.Verify(x => x.SetAsync(It.IsAny<DummyEntity>()), Times.Once);
		}

		[Fact]
		public async void Remove_Should_Invoke_Database_And_Cache()
		{
			var service = MoqHelper.CreateWithMocks<DummyRepository>();
			var IDatabaseService = Mock.Get(service.IDatabaseService);
			var IEntityCacheService = Mock.Get(service.IEntityCacheService);

			var entity = new DummyEntity();
			await service.RemoveAsync(entity);

			IDatabaseService.Verify(x => x.RemoveAsync<DummyEntity>(It.IsAny<string>()), Times.Once);
			IEntityCacheService.Verify(x => x.RemoveAsync<DummyEntity>(It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public async void Save_Should_Invoke_Database_And_Cache()
		{
			var service = MoqHelper.CreateWithMocks<DummyRepository>();
			var IDatabaseService = Mock.Get(service.IDatabaseService);
			var IEntityCacheService = Mock.Get(service.IEntityCacheService);

			var entity = new DummyEntity();
			await service.SaveAsync(entity);

			IDatabaseService.Verify(x => x.SaveAsync(It.IsAny<DummyEntity>()), Times.Once);
			IEntityCacheService.Verify(x => x.SetAsync(It.IsAny<DummyEntity>()), Times.Once);
		}

		[Fact]
		public async void GetByAutoId_Should_Invoke_IdTranslation()
		{
			var service = MoqHelper.CreateWithMocks<DummyRepository>();
			var IIdTranslationService = Mock.Get(service.IIdTranslationService);

			await service.GetByAutoIdAsync(1ul);

			IIdTranslationService.Verify(x => x.GetIdByAutoIdAsync<DummyEntity>(It.IsAny<ulong>()), Times.Once);
		}

		[Fact]
		public async void GetByName_Should_Invoke_Database()
		{
			var service = MoqHelper.CreateWithMocks<DummyRepository>();
			var IIdTranslationService = Mock.Get(service.IIdTranslationService);

			await service.GetByNameAsync("id");

			IIdTranslationService.Verify(x => x.GetIdByNameAsync<DummyEntity>(It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public void GetManyById_Should_Invoke_Entity_Cache()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void GetManyById_Should_Fetch_Uncached()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void PushTo_Cache_Should_Not_Set_Cached()
		{
			throw new NotImplementedException();
		}

		[Fact]
		public void PushTo_Cache_Should_Set_Uncached()
		{
			throw new NotImplementedException();
		}
	}
}
