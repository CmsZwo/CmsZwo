using System;
using System.Collections.Generic;

namespace CmsZwo.Repository
{
	public interface IDynamicCollectionFactory : IService
	{
		IDynamicCollection<T> Create<T>(
			IEnumerable<string> ids,
			Func<IEnumerable<T>, IEnumerable<T>> sorter = null
		)
			where T : IEntity;
	}

	public class DynamicCollectionFactory : Injectable, IDynamicCollectionFactory
	{
		#region Inject

		[Inject]
		public ICacheService ICacheService { get; set; }

		[Inject]
		public IContainerService IContainerService { get; set; }

		#endregion

		#region IDynamicCollectionFactory

		public IDynamicCollection<T> Create<T>(
			IEnumerable<string> ids,
			Func<IEnumerable<T>, IEnumerable<T>> sorter = null
		)
			where T : IEntity
		{
			var repository = IContainerService.Get<IRepository<T>>();
			ids = ids.WhereHasContent();

			var result =
				new DynamicCollection<T>(
					repository,
					ids,
					sorter
				);

			return result;
		}

		#endregion
	}
}
