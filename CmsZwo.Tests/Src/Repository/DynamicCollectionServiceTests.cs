using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Moq;
using Xunit;

using CmsZwo.Cache;

namespace CmsZwo.Repository.Tests
{
	public class DynamicCollectionServiceTests
	{
		public class DummyEntity : Entity { }

		public class DummyRepository : Repository<DummyEntity>
		{
		}

		[Fact]
		public void UpdateLists_Should_Remove_Old_Lists()
		{
			throw new System.NotImplementedException();
		}

		[Fact]
		public void UpdateLists_Should_Add_New_Lists()
		{
			throw new System.NotImplementedException();
		}

		[Fact]
		public void GetOrCreate_Should_Return_Cached()
		{
			throw new System.NotImplementedException();
		}

		[Fact]
		public async void GetOrCreate_Should_Create_Uncached()
		{
			var service = MoqHelper.CreateWithMocks<DynamicCollectionService>();

			var store = new List<DummyEntity> { new DummyEntity { } };

			var IRepository = new Mock<IRepository<DummyEntity>>();
			IRepository.Setup(x => x.GetQueryable()).Returns(store.AsQueryable());

			var IContainerService = Mock.Get(service.IContainerService);
			IContainerService.Setup(x => x.Get<IRepository<DummyEntity>>()).Returns(IRepository.Object);

			var IDatabaseService = Mock.Get(service.IDatabaseService);
			IDatabaseService.SetupIgnoreArgs(x => x.QueryAsync<DummyEntity>(null)).Returns(Task.FromResult(store));

			var IDynamicCollectionFactory = Mock.Get(service.IDynamicCollectionFactory);
			IDynamicCollectionFactory
				.SetupIgnoreArgs(x => x.Create<DummyEntity>(null, null))
				.Returns(new DynamicCollection<DummyEntity>(IRepository.Object, new[] { "1" }, null));

			var verify =
				await service.GetOrCreateAsync<DummyEntity>(
					"key",
					c => c
				);

			Assert.NotNull(verify);
		}
	}
}
