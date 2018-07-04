using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CmsZwo.Repository
{
	public interface IEntityCacheService : IService
	{
		Task<bool> IsCachedAsync<T>(T entity)
			where T : IEntity;

		Task SetAsync<T>(T entity)
			where T : IEntity;

		Task RemoveAsync<T>(string id)
			where T : IEntity;

		Task<T> GetByIdAsync<T>(string id)
			where T : IEntity;

		Task<IEnumerable<T>> GetManyByIdAsync<T>(IEnumerable<string> ids)
			where T : IEntity;

		Task Push<T>(IEnumerable<T> entities)
			where T : IEntity;
	}

	public class EntityCacheService : Injectable, IEntityCacheService
	{
		#region Inject

		[Inject]
		public ICacheService ICacheService { get; set; }

		#endregion

		#region Tools

		private string GetKey<T>(string id)
			=> $"{nameof(EntityCacheService)}.{typeof(T).Name}:{id}";

		private string GetKey<T>(T entity)
			where T : IEntity
			=> GetKey<T>(entity.Id);

		#endregion

		#region IEntityCacheService

		public Task<bool> IsCachedAsync<T>(T entity)
			where T : IEntity
			=> ICacheService.IsCachedAsync(GetKey(entity));

		public Task<T> GetByIdAsync<T>(string id)
			where T : IEntity
			=> ICacheService.GetAsync<T>(GetKey<T>(id));

		public Task SetAsync<T>(T entity)
			where T : IEntity
			=> ICacheService.SetAsync(GetKey(entity), entity);

		public Task RemoveAsync<T>(string id)
			where T : IEntity
			=> ICacheService.RemoveAsync(GetKey<T>(id));

		public Task<IEnumerable<T>> GetManyByIdAsync<T>(IEnumerable<string> ids)
			where T : IEntity
			=> ICacheService.GetManyAsync<T>(ids.Select(x => GetKey<T>(x)));

		public async Task Push<T>(IEnumerable<T> entities)
				where T : IEntity
		{
			foreach (var entity in entities)
			{
				var isCached = await IsCachedAsync(entity);
				if (isCached) continue;

				await SetAsync(entity);
			}
		}

		#endregion
	}
}
