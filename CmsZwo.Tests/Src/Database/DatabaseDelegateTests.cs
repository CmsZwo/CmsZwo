using System.Linq;
using System.Threading.Tasks;

using Moq;
using Xunit;

using CmsZwo.Database;
using System.Collections.Generic;

namespace CmsZwo.Repository.Tests
{
	public class DatabaseDelegateTests
	{
		private class DummyEntity : Entity { }

		public class DummyDatabaseDelegate : DatabaseDelegate
		{
			public bool WillInsertCalled { get; set; }
			public bool DidInsertCalled { get; set; }

			protected override Task WillInsertAsync<T>(T entity)
			{
				WillInsertCalled = true;
				return base.WillInsertAsync(entity);
			}

			protected override Task DidInsertAsync<T>(T entity)
			{
				DidInsertCalled = true;
				return base.DidInsertAsync(entity);
			}

			protected override Task DoInsertAsync<T>(T entity)
				=> Task.CompletedTask;

			protected override Task DoRemoveAsync<T>(string id)
				=> Task.CompletedTask;

			protected override Task DoSaveAsync<T>(T entity)
				=> Task.CompletedTask;

			public override IQueryable<T> GetQueryable<T>()
				=> null;

			public override Task<T> GetByIdAsync<T>(string id)
				=> Task.FromResult(default(T));

			public override Task<IEnumerable<T>> GetManyByIdAsync<T>(IEnumerable<string> ids)
				=> Task.FromResult(default(IEnumerable<T>));

			public override Task<List<T>> QueryAsync<T>(IQueryable<T> queryable)
				=> Task.FromResult(queryable.ToList());
		}

		[Fact]
		public async void Insert_Should_Invole_Will_And_Did()
		{
			var service = MoqHelper.CreateWithMocks<DummyDatabaseDelegate>();

			var dummy = new DummyEntity();
			await service.InsertAsync(dummy);

			Assert.True(service.WillInsertCalled);
			Assert.True(service.DidInsertCalled);
		}
	}
}
