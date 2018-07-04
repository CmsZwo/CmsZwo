using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using CmsZwo.Database;
using CmsZwo.Repository;

namespace CmsZwo
{
	public interface IRepository : IService { }

	public interface IRepository<T> : IRepository
		where T : IEntity
	{
		IQueryable<T> GetQueryable();

		Task InsertAsync(T entity);
		Task SaveAsync(T entity);
		Task RemoveAsync(T entity);

		Task<T> GetByIdAsync(string id);
		Task<IEnumerable<T>> GetManyByIdAsync(IEnumerable<string> ids);

		Task<T> GetByNameAsync(string name);
		Task<T> GetByAutoIdAsync(ulong autoId);
	}

	public abstract class Repository<T> : Injectable, IRepository<T>
		where T : IEntity
	{
		#region Inject

		[Inject]
		public ITimeService ITimeService { get; set; }

		[Inject]
		public IAutoIdService IAutoIdService { get; set; }

		[Inject]
		public IEntityCacheService IEntityCacheService { get; set; }

		[Inject]
		public IDatabaseService IDatabaseService { get; set; }

		[Inject]
		public IIdTranslationService IIdTranslationService { get; set; }

		[Inject]
		public ICacheEventService ICacheEventService { get; set; }

		[Inject]
		public IDynamicCollectionService IDynamicCollectionService { get; set; }

		#endregion

		#region IRepository<T>

		public IQueryable<T> GetQueryable()
			=> IDatabaseService.GetQueryable<T>();

		public async Task<T> GetByIdAsync(string id)
		{
			var result = await IEntityCacheService.GetByIdAsync<T>(id);

			if (result == null)
			{
				result = await IDatabaseService.GetByIdAsync<T>(id);
				if (result != null)
					await IEntityCacheService.SetAsync(result);
			}

			return result;
		}

		public async Task<IEnumerable<T>> GetManyByIdAsync(IEnumerable<string> ids)
		{
			var cached = await IEntityCacheService.GetManyByIdAsync<T>(ids);
			var cachedIds = cached.Select(x => x.Id);

			var uncachedIds = ids.Except(cachedIds);
			var uncached = await IDatabaseService.GetManyByIdAsync<T>(uncachedIds);
			await IEntityCacheService.Push(uncached);

			cached = await IEntityCacheService.GetManyByIdAsync<T>(ids);
			return cached;
		}

		public async Task InsertAsync(T entity)
		{
			entity.Created = ITimeService.Now;
			entity.AutoId = await IAutoIdService.GetNext<T>();

			await IDatabaseService.InsertAsync(entity);
			await IEntityCacheService.SetAsync(entity);
		}

		public async Task RemoveAsync(T entity)
		{
			await IDatabaseService.RemoveAsync<T>(entity.Id);
			await IEntityCacheService.RemoveAsync<T>(entity.Id);
		}

		public async Task SaveAsync(T entity)
		{
			await IDatabaseService.SaveAsync(entity);
			await IEntityCacheService.SetAsync(entity);
		}

		public async Task<T> GetByAutoIdAsync(ulong autoId)
		{
			var id = await IIdTranslationService.GetIdByAutoIdAsync<T>(autoId);
			return await IEntityCacheService.GetByIdAsync<T>(id);
		}

		public async Task<T> GetByNameAsync(string name)
		{
			var id = await IIdTranslationService.GetIdByNameAsync<T>(name);
			return await IEntityCacheService.GetByIdAsync<T>(id);
		}

		#endregion
	}
}
