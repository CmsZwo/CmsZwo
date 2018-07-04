using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace CmsZwo.Repository
{
	public interface IDynamicCollection<T> : IEnumerable<T>
		where T : IEntity
	{
		void Add(T entity);
		void Remove(T entity);
	}

	public class DynamicCollection<T> : IDynamicCollection<T>
		where T : IEntity
	{
		#region Construct

		private IRepository<T> _IRepository { get; set; }
		private List<string> _Ids { get; set; }
		private readonly Func<IEnumerable<T>, IEnumerable<T>> _Sorter;

		public DynamicCollection(
			IRepository<T> repository,
			IEnumerable<string> ids,
			Func<IEnumerable<T>, IEnumerable<T>> sorter
			)
		{
			_IRepository = repository;
			_Ids = new List<string>(ids);
			_Sorter = sorter;
		}

		#endregion

		#region Tools

		private void Sort()
		{
			if (_Sorter == null)
				return;

			var entites = this.AsEnumerable();
			var sorted = _Sorter(entites);

			_Ids =
				sorted
					.Select(x => x.Id)
					.ToList();
		}

		#endregion

		#region IDynamicCollection<T>

		public IEnumerator<T> GetEnumerator()
		{
			var removes = null as List<string>;

			foreach (var id in _Ids)
			{
				var entity =
					_IRepository
						.GetByIdAsync(id)
						.GetAwaiter()
						.GetResult();

				if (entity != null)
				{
					yield return entity;
					continue;
				}

				if (removes == null)
					removes = new List<string>();

				removes.Add(id);
			}

			foreach (var id in removes.Safe())
				_Ids.Remove(id);
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public void Add(T entity)
		{
			if (_Ids.Contains(entity.Id))
				return;

			_Ids.Add(entity.Id);
			Sort();
		}

		public void Remove(T entity)
		{
			if (!_Ids.Contains(entity.Id))
				return;

			_Ids.Remove(entity.Id);
			Sort();
		}

		#endregion
	}
}

