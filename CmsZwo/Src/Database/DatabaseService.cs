using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using CmsZwo.Database.Memory;

namespace CmsZwo.Database
{
	public interface IDatabaseService : IDatabaseDelegate { }

	public class DatabaseService : Injectable, IDatabaseService
	{
		#region Inject

		[Inject]
		public IMemoryDatabaseDelegate IMemoryDatabaseDelegate { get; set; }

		#endregion

		#region Tools

		private IDatabaseDelegate _IDatabaseDelegate;
		private IDatabaseDelegate IDatabaseDelegate
		{
			get
			{
				if (_IDatabaseDelegate == null)
					_IDatabaseDelegate = IMemoryDatabaseDelegate;

				return _IDatabaseDelegate;
			}
		}

		#endregion

		#region IDatabase

		public IQueryable<T> GetQueryable<T>()
			where T : IEntity
			=> IDatabaseDelegate.GetQueryable<T>();

		public Task<T> GetByIdAsync<T>(string id)
			where T : IEntity
			=> IDatabaseDelegate.GetByIdAsync<T>(id);

		public Task InsertAsync<T>(T entity)
			where T : IEntity
			=> IDatabaseDelegate.InsertAsync(entity);

		public Task RemoveAsync<T>(string id)
			where T : IEntity
			=> IDatabaseDelegate.RemoveAsync<T>(id);

		public Task SaveAsync<T>(T entity)
			where T : IEntity
			=> IDatabaseDelegate.SaveAsync(entity);

		public Task<IEnumerable<T>> GetManyByIdAsync<T>(IEnumerable<string> ids)
			where T : IEntity
			=>
			Task.Run(() =>
			{
				var result =
					IDatabaseDelegate
						.GetQueryable<T>()
						.Where(x => ids.Contains(x.Id))
						.AsEnumerable();

				return
					Task.FromResult(result);
			});

		public Task<List<T>> QueryAsync<T>(IQueryable<T> queryable)
			where T : IEntity
			=> IDatabaseDelegate.QueryAsync(queryable);

		#endregion
	}
}
