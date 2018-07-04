using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CmsZwo.Database.Memory
{
	public interface IMemoryDatabaseDelegate : IDatabaseDelegate { }

	public class MemoryDatabaseDelegate : DatabaseDelegate, IMemoryDatabaseDelegate
	{
		#region Inject

		[Inject]
		public IMemoryCollectionFactory IMemoryCollectionFactory { get; set; }

		#endregion

		#region IDatabase

		public override IQueryable<T> GetQueryable<T>()
			=> IMemoryCollectionFactory.GetOrCreateCollection<T>().AsQueryable();

		public override Task<List<T>> QueryAsync<T>(IQueryable<T> queryable)
			=> Task.FromResult(queryable.ToList());

		public override Task<T> GetByIdAsync<T>(string id)
		{
			var collection = IMemoryCollectionFactory.GetOrCreateCollection<T>();

			return Task.FromResult(
				collection
					.FirstOrDefault(x => x.Id == id)
					.CopyByJson()
			);
		}

		public override Task<IEnumerable<T>> GetManyByIdAsync<T>(IEnumerable<string> ids)
		{
			var collection = IMemoryCollectionFactory.GetOrCreateCollection<T>();

			return Task.FromResult(
				collection
					.Where(x => ids.Contains(x.Id))
					.ToList()
					.AsEnumerable()
					.CopyByJson()
			);
		}

		protected override Task DoInsertAsync<T>(T entity)
		{
			var collection = IMemoryCollectionFactory.GetOrCreateCollection<T>();

			entity.Id = IRandomService.RandomBase24(12);
			entity.Created = ITimeService.Now;

			collection.Add(entity.CopyByJson());

			return Task.CompletedTask;
		}

		protected override Task DoRemoveAsync<T>(string id)
		{
			var collection = IMemoryCollectionFactory.GetOrCreateCollection<T>();
			collection.RemoveAll(x => x.Id == id);
			return Task.CompletedTask;
		}

		protected override async Task DoSaveAsync<T>(T entity)
		{
			if (!entity.Id.HasContent())
			{
				await DoInsertAsync(entity);
				return;
			}

			await DoRemoveAsync<T>(entity.Id);
			var collection = IMemoryCollectionFactory.GetOrCreateCollection<T>();
			collection.Add(entity.CopyByJson());
		}

		#endregion
	}
}
