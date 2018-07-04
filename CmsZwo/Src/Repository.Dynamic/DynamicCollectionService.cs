using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using CmsZwo.Database;
using CmsZwo.Cache.Memory;

namespace CmsZwo.Repository
{
	public interface IDynamicCollectionService : IService
	{
		Task UpdateListsAsync<T>(T entity)
			where T : IEntity;

		Task<IDynamicCollection<T>> GetOrCreateAsync<T>(
			string listName,
			Func<IQueryable<T>, IQueryable<T>> query = null,
			Func<IEnumerable<T>, IEnumerable<T>> sorter = null
		)
			where T : IEntity;
	}

	public class DynamicCollectionService : Injectable, IDynamicCollectionService
	{
		#region Inject

		[Inject]
		public IContainerService IContainerService { get; set; }

		[Inject]
		public ICacheService ICacheService { get; set; }

		[Inject]
		public IMemoryCacheService IMemoryCacheService { get; set; }

		[Inject]
		public IDynamicCollectionFactory IDynamicCollectionFactory { get; set; }

		[Inject]
		public IEntityCacheService IEntityCacheService { get; set; }

		[Inject]
		public IDatabaseService IDatabaseService { get; set; }

		#endregion

		#region Tools

		private string GetCollectionCacheKey(string listName)
			=> $"{nameof(DynamicCollectionService)}:{listName}";

		private string GetIdsCacheKey(string listName)
			=> $"{nameof(DynamicCollectionService)}-Ids:{listName}";

		private IEnumerable<string> GetLists<T>(T entity)
			where T : IEntity
		{
			var repository = IContainerService.Get<IRepository<T>>();

			if (!(repository is IDynamicRepository<T> dynamicRepository))
				throw new Exception($"{repository.GetType().Name} must implement {nameof(IDynamicRepository<T>)}");

			var result = dynamicRepository.GetDynamicListNames(entity);
			return result;
		}

		private string GetListKey<T>(T entity)
			where T : IEntity
			=> $"{nameof(DynamicCollectionFactory)}-{nameof(GetListKey)}:{entity.Id}";

		private Task<HashSet<string>> GetCachedListNamesAsync<T>(T entity)
			where T : IEntity
			=>
				ICacheService.GetSetAsync(
					GetListKey(entity),
					() => GetLists(entity)
				);

		private Task AddListNamesAsync<T>(T entity, IEnumerable<string> listNames)
			where T : IEntity
			=>
				ICacheService.AddToSetAsync(
					GetListKey(entity),
					listNames
				);

		private Task RemoveListNamesAsync<T>(T entity, IEnumerable<string> listNames)
			where T : IEntity
			=>
				ICacheService.AddToSetAsync(
					GetListKey(entity),
					listNames
				);

		#endregion

		#region IDynamicCollectionService

		public async Task UpdateListsAsync<T>(T entity)
			where T : IEntity
		{
			var oldLists = await GetCachedListNamesAsync(entity);
			var newLists = GetLists(entity);

			var removes = oldLists.Except(newLists);
			var add = newLists.Except(oldLists);

			await RemoveListNamesAsync(entity, removes);
			await AddListNamesAsync(entity, add);
		}

		public async Task<IDynamicCollection<T>> GetOrCreateAsync<T>(
			string listName,
			Func<IQueryable<T>, IQueryable<T>> query = null,
			Func<IEnumerable<T>, IEnumerable<T>> sorter = null
			)
			where T : IEntity
		{
			var collectionKey = GetCollectionCacheKey(listName);

			var result =
				IMemoryCacheService.Get<IDynamicCollection<T>>(collectionKey);

			if (result != null)
				return result;

			var IRepository = IContainerService.Get<IRepository<T>>();

			var idsKey = GetIdsCacheKey(listName);
			var entityIds = await ICacheService.GetAsync<List<string>>(idsKey);

			if (entityIds == null)
			{
				var queryable = IRepository.GetQueryable();

				if (query != null)
					queryable = query(queryable);

				var entities = await IDatabaseService.QueryAsync(queryable);

				await IEntityCacheService.Push(entities);

				entityIds =
					entities
						.Select(x => x?.Id)
						.ToList();
			}
			else
			{
				await IRepository.GetManyByIdAsync(entityIds);
			}

			result =
				IDynamicCollectionFactory
					.Create(entityIds, sorter);

			IMemoryCacheService.Set(collectionKey, result);

			return result;
		}

		#endregion
	}
}
