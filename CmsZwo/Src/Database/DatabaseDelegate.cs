using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CmsZwo.Database
{
	public interface IDatabaseDelegate : IService
	{
		IQueryable<T> GetQueryable<T>()
		where T : IEntity;

		Task<List<T>> QueryAsync<T>(IQueryable<T> queryable)
			where T : IEntity;

		Task InsertAsync<T>(T entity)
			where T : IEntity;

		Task SaveAsync<T>(T entity)
			where T : IEntity;

		Task RemoveAsync<T>(string id)
			where T : IEntity;

		Task<T> GetByIdAsync<T>(string id)
			where T : IEntity;

		Task<IEnumerable<T>> GetManyByIdAsync<T>(IEnumerable<string> ids)
			where T : IEntity;
	}

	public abstract class DatabaseDelegate : Injectable, IDatabaseDelegate
	{
		#region Inject

		[Inject]
		public IRandomService IRandomService { get; set; }

		[Inject]
		public ITimeService ITimeService { get; set; }

		#endregion

		#region Tools

		protected virtual Task WillInsertAsync<T>(T entity)
			where T : IEntity
			=> Task.CompletedTask;

		protected virtual Task DidInsertAsync<T>(T entity)
			where T : IEntity
			=> Task.CompletedTask;

		protected abstract Task DoInsertAsync<T>(T entity)
			where T : IEntity;

		protected abstract Task DoRemoveAsync<T>(string id)
			where T : IEntity;

		protected abstract Task DoSaveAsync<T>(T entity)
			where T : IEntity;

		#endregion

		#region IDatabase

		public abstract IQueryable<T> GetQueryable<T>()
			where T : IEntity;

		public abstract Task<List<T>> QueryAsync<T>(IQueryable<T> queryable)
			where T : IEntity;

		public abstract Task<T> GetByIdAsync<T>(string id)
			where T : IEntity;

		public abstract Task<IEnumerable<T>> GetManyByIdAsync<T>(IEnumerable<string> ids)
			where T : IEntity;

		public async Task InsertAsync<T>(T entity)
			where T : IEntity
		{
			await WillInsertAsync(entity);
			await DoInsertAsync(entity);
			await DidInsertAsync(entity);
		}

		public Task SaveAsync<T>(T entity)
			where T : IEntity
			=> DoSaveAsync(entity);

		public Task RemoveAsync<T>(string id)
			where T : IEntity
			=> DoRemoveAsync<T>(id);

		#endregion
	}
}
