using System.Threading.Tasks;

namespace CmsZwo.Repository
{
	public interface IAutoIdService : IService
	{
		Task<ulong> GetNext<T>()
			where T : IEntity;
	}

	public class AutoIdService : Injectable, IAutoIdService
	{
		#region Inject

		[Inject]
		public IAutoIdRepository IAutoIdRepository { get; set; }

		#endregion

		#region Tools

		private string GetName<T>()
			where T : IEntity
			=> $"{nameof(AutoIdService)}:{typeof(T).Name}";

		private async Task<AutoId> GetOrCreateAutoId<T>()
			where T : IEntity
		{
			var name = GetName<T>();

			var result = await IAutoIdRepository.GetByNameAsync(name);
			if (result != null)
				return result;

			result = new AutoId
			{
				Name = name,
				Value = 0ul
			};

			await IAutoIdRepository.SaveAsync(result);
			return result;
		}

		#endregion

		#region IAutoIdService<T>

		public async Task<ulong> GetNext<T>()
			where T : IEntity
		{
			var autoId = await GetOrCreateAutoId<T>();
			var result = ++autoId.Value;
			await IAutoIdRepository.SaveAsync(autoId);
			return result;
		}

		#endregion
	}
}
