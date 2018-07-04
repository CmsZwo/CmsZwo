using System.Collections.Generic;

namespace CmsZwo
{
	public interface IDynamicRepository<T>
		where T : IEntity
	{
		IEnumerable<string> GetDynamicListNames(T entity);
	}
}
