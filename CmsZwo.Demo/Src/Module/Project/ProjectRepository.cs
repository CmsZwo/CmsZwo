using System.Threading.Tasks;
using System.Collections.Generic;

namespace CmsZwo.Demo
{
	public interface IProjectRepository : IRepository<Project>, IDynamicRepository<Project>
	{
		Task<IEnumerable<Project>> GetDynamic();
	}

	public class ProjectRepository : Repository<Project>, IProjectRepository
	{
		#region IProjectRepository

		public IEnumerable<string> GetDynamicListNames(Project entity)
			=> new[] { $"{nameof(ProjectRepository)}:{nameof(GetDynamic)}" };

		public async Task<IEnumerable<Project>> GetDynamic()
		{
			var result =
				await
					IDynamicCollectionService.GetOrCreateAsync<Project>(
						$"{nameof(ProjectRepository)}:{nameof(GetDynamic)}"
					);

			return result;
		}

		#endregion
	}
}
