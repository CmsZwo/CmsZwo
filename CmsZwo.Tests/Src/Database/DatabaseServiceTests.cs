using Moq;
using Xunit;

using CmsZwo.Database;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CmsZwo.Repository.Tests
{
	public class DatabaseServiceTests
	{
		private class DummyEntity : Entity { }
		private class DummyDatabaseServnice : DatabaseService { }

		[Fact]
		public void GetQueryable_Should_Invoke_Delegate()
		{
			var service = MoqHelper.CreateWithMocks<DummyDatabaseServnice>();
			var IDatabaseDelegate = Mock.Get(service.IMemoryDatabaseDelegate);

			service.GetQueryable<DummyEntity>();
			IDatabaseDelegate.Verify(x => x.GetQueryable<DummyEntity>(), Times.Once);
		}

		[Fact]
		public void Insert_Should_Invoke_Delegate()
		{
			var service = MoqHelper.CreateWithMocks<DummyDatabaseServnice>();
			var IDatabaseDelegate = Mock.Get(service.IMemoryDatabaseDelegate);

			service.InsertAsync(new DummyEntity());
			IDatabaseDelegate.Verify(x => x.InsertAsync(It.IsAny<DummyEntity>()), Times.Once);
		}

		[Fact]
		public void Save_Should_Invoke_Delegate()
		{
			var service = MoqHelper.CreateWithMocks<DummyDatabaseServnice>();
			var IDatabaseDelegate = Mock.Get(service.IMemoryDatabaseDelegate);

			service.SaveAsync(new DummyEntity());
			IDatabaseDelegate.Verify(x => x.SaveAsync(It.IsAny<DummyEntity>()), Times.Once);
		}

		[Fact]
		public void Remove_Should_Invoke_Delegate()
		{
			var service = MoqHelper.CreateWithMocks<DummyDatabaseServnice>();
			var IDatabaseDelegate = Mock.Get(service.IMemoryDatabaseDelegate);

			service.RemoveAsync<DummyEntity>("id");
			IDatabaseDelegate.Verify(x => x.RemoveAsync<DummyEntity>(It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public void GetById_Should_Invoke_Delegate()
		{
			var service = MoqHelper.CreateWithMocks<DummyDatabaseServnice>();
			var IDatabaseDelegate = Mock.Get(service.IMemoryDatabaseDelegate);

			service.GetByIdAsync<DummyEntity>("id");
			IDatabaseDelegate.Verify(x => x.GetByIdAsync<DummyEntity>(It.IsAny<string>()), Times.Once);
		}

		[Fact]
		public void Query_Invoke_Delegate()
		{
			throw new System.NotImplementedException();
		}
	}
}
