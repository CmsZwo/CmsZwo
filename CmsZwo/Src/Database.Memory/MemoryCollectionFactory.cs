using System;
using System.Collections.Generic;

namespace CmsZwo.Database.Memory
{
	public interface IMemoryCollectionFactory : IService
	{
		List<T> GetOrCreateCollection<T>();
	}

	public class MemoryCollectionFactory : IMemoryCollectionFactory
	{
		#region Tools

		private readonly Dictionary<Type, object> _Collections
			= new Dictionary<Type, object>();

		#endregion

		#region IMemoryCollectionFactory

		public List<T> GetOrCreateCollection<T>()
		{
			var type = typeof(T);

			if (!_Collections.ContainsKey(type))
				_Collections[type] = new List<T>();

			return _Collections[type] as List<T>;
		}

		#endregion
	}
}
