using System;
using System.Linq;
using System.Threading.Tasks;

using CmsZwo.Database;

namespace CmsZwo.Repository
{
	public interface IIdTranslationService : IService
	{
		Task<string> GetIdByNameAsync<T>(string name)
			where T : IEntity;

		Task<string> GetIdByAutoIdAsync<T>(ulong autoId)
			where T : IEntity;

		Task PresetNameAsync<T>(T entity)
			where T : IEntity;

		Task PresetAutoIdAsync<T>(T entity)
			where T : IEntity;
	}

	public class IdTranslationService : Injectable, IIdTranslationService
	{
		#region Inject

		[Inject]
		public IDatabaseService IDatabaseService { get; set; }

		[Inject]
		public ICacheService ICacheService { get; set; }

		#endregion

		#region Tools

		private async Task<string> GetIdByTranslation<T>(
			string keyName,
			string value,
			Func<IQueryable<T>, string> factory
		)
			where T : IEntity
		{
			if (!value.HasContent())
				return null;

			return
				await ICacheService.GetAsync
				(
					GetKey(keyName, value),
					() =>
					{
						var query = IDatabaseService.GetQueryable<T>();
						var _result = factory(query);
						return _result;
					}
				);
		}

		private string GetKey(string keyName, string value)
				=> $"{nameof(IdTranslationService)}:{keyName}-{value.ToLower()}";

		#endregion

		#region IIdTranslationService

		public Task<string> GetIdByNameAsync<T>(string name)
			where T : IEntity
		{
			if (!name.HasContent())
				return Task.FromResult<string>(null);

			return
				GetIdByTranslation<T>(
					nameof(GetIdByNameAsync),
					name,
					x =>
						x
							.FirstOrDefault(c => c.Name.ToLower() == name.ToLower())
							?.Id
				);
		}

		public Task<string> GetIdByAutoIdAsync<T>(ulong autoId)
			where T : IEntity
		{
			if (autoId < 1)
				return Task.FromResult<string>(null);

			return
				GetIdByTranslation<T>(
						nameof(GetIdByAutoIdAsync),
						autoId.ToString(),
						x =>
							x
								.FirstOrDefault(c => c.AutoId == autoId)
								?.Id
					);
		}

		public Task PresetNameAsync<T>(T entity)
			where T : IEntity
		{
			if (entity?.Id.HasContent() != true)
				return Task.CompletedTask;

			if (entity?.Name.HasContent() != true)
				return Task.CompletedTask;

			return
				ICacheService
					.SetAsync(
						GetKey(nameof(GetIdByNameAsync), entity.Name),
						entity.Id
					);
		}

		public Task PresetAutoIdAsync<T>(T entity)
			where T : IEntity
		{
			if (entity?.Id.HasContent() != true)
				return Task.CompletedTask;

			if (entity?.AutoId < 1ul)
				return Task.CompletedTask;

			return
				ICacheService
					.SetAsync(
						GetKey(nameof(GetIdByAutoIdAsync), entity.AutoId.ToString()),
						entity.Id
					);
		}

		#endregion
	}
}
