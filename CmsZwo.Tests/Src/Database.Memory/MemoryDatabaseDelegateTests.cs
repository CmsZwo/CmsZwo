using System.Linq;
using System.Collections.Generic;

using Moq;
using Xunit;

using CmsZwo.Database.Memory;

namespace CmsZwo.Repository.Tests
{
	public class MemoryDatabaseDelegateTests
	{
		private class DummyEntity : Entity { }

		[Fact]
		public void GetQueryable()
		{
			var service = MoqHelper.CreateWithMocks<MemoryDatabaseDelegate>();

			var store = new List<DummyEntity>
			{
				new DummyEntity
				{
					Id = "1",
					Name = "Foo"
				},
				new DummyEntity
				{
					Id = "2",
					Name = "Bar"
				}
			};

			var IMemoryCollectionFactory = Mock.Get(service.IMemoryCollectionFactory);
			IMemoryCollectionFactory.Setup(x => x.GetOrCreateCollection<DummyEntity>()).Returns(store);

			var query = service.GetQueryable<DummyEntity>();
			Assert.Equal(2, query.Count());
		}

		[Fact]
		public async void Insert()
		{
			var service = MoqHelper.CreateWithMocks<MemoryDatabaseDelegate>();

			var IRandomService = Mock.Get(service.IRandomService);
			IRandomService.Setup(x => x.RandomBase24(It.IsAny<int>(), It.IsAny<bool>())).Returns("123");

			var store = new List<DummyEntity>();
			var IMemoryCollectionFactory = Mock.Get(service.IMemoryCollectionFactory);
			IMemoryCollectionFactory.Setup(x => x.GetOrCreateCollection<DummyEntity>()).Returns(store);

			var entity = new DummyEntity();
			await service.InsertAsync(entity);

			Assert.Contains(store, x => x.Id == "123");
		}

		[Fact]
		public async void Save()
		{
			var service = MoqHelper.CreateWithMocks<MemoryDatabaseDelegate>();

			var store = new List<DummyEntity>
			{
				new DummyEntity
				{
					Id = "123",
					Name = "Foo"
				}
			};

			var IMemoryCollectionFactory = Mock.Get(service.IMemoryCollectionFactory);
			IMemoryCollectionFactory.Setup(x => x.GetOrCreateCollection<DummyEntity>()).Returns(store);

			var entity = new DummyEntity
			{
				Id = "123",
				Name = "Bar"
			};

			await service.SaveAsync(entity);

			Assert.Contains(store, x => x.Name == "Bar");
		}

		[Fact]
		public async void Remove()
		{
			var service = MoqHelper.CreateWithMocks<MemoryDatabaseDelegate>();

			var store = new List<DummyEntity>
			{
				new DummyEntity
				{
					Id = "123",
					Name = "Foo"
				}
			};

			var IMemoryCollectionFactory = Mock.Get(service.IMemoryCollectionFactory);
			IMemoryCollectionFactory.Setup(x => x.GetOrCreateCollection<DummyEntity>()).Returns(store);

			var entity = new DummyEntity
			{
				Id = "123"
			};

			await service.RemoveAsync<DummyEntity>(entity.Id);

			Assert.DoesNotContain(store, x => x.Id == "123");
		}

		[Fact]
		public async void GetById()
		{
			var service = MoqHelper.CreateWithMocks<MemoryDatabaseDelegate>();

			var store = new List<DummyEntity>
			{
				new DummyEntity
				{
					Id = "123"
				}
			};

			var IMemoryCollectionFactory = Mock.Get(service.IMemoryCollectionFactory);
			IMemoryCollectionFactory.Setup(x => x.GetOrCreateCollection<DummyEntity>()).Returns(store);

			var verify = await service.GetByIdAsync<DummyEntity>("123");
			Assert.NotNull(verify);
		}

		[Fact]
		public void GetManyById()
		{
			throw new System.NotImplementedException();
		}

		[Fact]
		public void Query()
		{
			throw new System.NotImplementedException();
		}
	}
}
