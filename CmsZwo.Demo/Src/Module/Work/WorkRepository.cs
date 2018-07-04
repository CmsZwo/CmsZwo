using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CmsZwo.Demo
{
	public interface IWorkRepository : IRepository<Work>, IDynamicRepository<Work>
	{
		Task<IEnumerable<Work>> GetDynamic(Project project);
	}

	public class WorkRepository : Repository<Work>, IWorkRepository
	{
		#region IProjectRepository

		public IEnumerable<string> GetDynamicListNames(Work entity)
		{
			if (!entity.ProjectId.HasContent())
				return null;

			return new[]
			{
				$"{nameof(WorkRepository)}:{entity.ProjectId}"
			};
		}

		public async Task<IEnumerable<Work>> GetDynamic(Project project)
		{
			var result =
				await
					IDynamicCollectionService.GetOrCreateAsync<Work>(
						$"{nameof(WorkRepository)}:{project.Id}",
						c => c.Where(x => x.ProjectId == project.Id)
					);

			return result;
		}

		#endregion
	}
}
