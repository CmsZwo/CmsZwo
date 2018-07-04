using System;
using System.Collections.Generic;

namespace CmsZwo
{
	public interface ISettingsService : IService
	{
		T Get<T>()
			where T : class;

		void Set<T>(T settings)
			where T : class;
	}

	public class SettingsService : ISettingsService
	{
		private Dictionary<Type, object> _Settings
			= new Dictionary<Type, object>();

		public T Get<T>()
			where T : class
		{
			var type = typeof(T);

			if (!_Settings.ContainsKey(type))
				return default(T);

			return _Settings[type] as T;
		}

		public void Set<T>(T settings)
			where T : class
		{
			var type = typeof(T);
			_Settings[type] = settings;
		}
	}
}
